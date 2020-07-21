using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public class UrlPathPlaceholderNameAndValueFinder : IPlaceholderNameAndValueFinder
    {
        public Response<List<PlaceholderNameAndValue>> Find(string path, string query, string pathTemplate)
        {
            var placeHolderNameAndValues = new List<PlaceholderNameAndValue>();

            path = $"{path}{query}";

            int counterForPath = 0;

            var delimiter = '/';
            var nextDelimiter = '/';

            for (int counterForTemplate = 0; counterForTemplate < pathTemplate.Length; counterForTemplate++)
            {
                if ((path.Length > counterForPath) && CharactersDontMatch(pathTemplate[counterForTemplate], path[counterForPath]) && ContinueScanningUrl(counterForPath, path.Length))
                {
                    if (IsPlaceholder(pathTemplate[counterForTemplate]))
                    {
                        //should_find_multiple_query_string make test pass
                        if (PassedQueryString(pathTemplate, counterForTemplate))
                        {
                            delimiter = '&';
                            nextDelimiter = '&';
                        }

                        //should_find_multiple_query_string_and_path makes test pass
                        if (NotPassedQueryString(pathTemplate, counterForTemplate) && NoMoreForwardSlash(pathTemplate, counterForTemplate))
                        {
                            delimiter = '?';
                            nextDelimiter = '?';
                        }

                        var placeholderName = GetPlaceholderName(pathTemplate, counterForTemplate);

                        var placeholderValue = GetPlaceholderValue(pathTemplate, query, placeholderName, path, counterForPath, delimiter);

                        placeHolderNameAndValues.Add(new PlaceholderNameAndValue(placeholderName, placeholderValue));

                        counterForTemplate = GetNextCounterPosition(pathTemplate, counterForTemplate, '}');

                        counterForPath = GetNextCounterPosition(path, counterForPath, nextDelimiter);

                        continue;
                    }

                    return new OkResponse<List<PlaceholderNameAndValue>>(placeHolderNameAndValues);
                }
                else if (IsCatchAll(path, counterForPath, pathTemplate))
                {
                    var endOfPlaceholder = GetNextCounterPosition(pathTemplate, counterForTemplate, '}');

                    var placeholderName = GetPlaceholderName(pathTemplate, 1);

                    if (NothingAfterFirstForwardSlash(path))
                    {
                        placeHolderNameAndValues.Add(new PlaceholderNameAndValue(placeholderName, ""));
                    }
                    else
                    {
                        var placeholderValue = GetPlaceholderValue(pathTemplate, query, placeholderName, path, counterForPath + 1, '?');
                        placeHolderNameAndValues.Add(new PlaceholderNameAndValue(placeholderName, placeholderValue));
                    }

                    counterForTemplate = endOfPlaceholder;
                }

                counterForPath++;
            }

            return new OkResponse<List<PlaceholderNameAndValue>>(placeHolderNameAndValues);
        }

        private static bool NoMoreForwardSlash(string pathTemplate, int counterForTemplate)
        {
            return !pathTemplate.Substring(counterForTemplate).Contains("/");
        }

        private static bool NotPassedQueryString(string pathTemplate, int counterForTemplate)
        {
            return !pathTemplate.Substring(0, counterForTemplate).Contains("?");
        }

        private static bool PassedQueryString(string pathTemplate, int counterForTemplate)
        {
            return pathTemplate.Substring(0, counterForTemplate).Contains("?");
        }

        private bool IsCatchAll(string path, int counterForPath, string pathTemplate)
        {
            return string.IsNullOrEmpty(path) || (path.Length > counterForPath && path[counterForPath] == '/') && pathTemplate.Length > 1
                     && pathTemplate.Substring(0, 2) == "/{"
                     && pathTemplate.IndexOf('}') == pathTemplate.Length - 1;
        }

        private bool NothingAfterFirstForwardSlash(string path)
        {
            return path.Length == 1 || path.Length == 0;
        }

        private string GetPlaceholderValue(string urlPathTemplate, string query, string variableName, string urlPath, int counterForUrl, char delimiter)
        {
            var positionOfNextSlash = urlPath.IndexOf(delimiter, counterForUrl);

            if (positionOfNextSlash == -1 || (urlPathTemplate.Trim(delimiter).EndsWith(variableName) && string.IsNullOrEmpty(query)))
            {
                positionOfNextSlash = urlPath.Length;
            }

            var variableValue = urlPath.Substring(counterForUrl, positionOfNextSlash - counterForUrl);

            return variableValue;
        }

        private string GetPlaceholderName(string urlPathTemplate, int counterForTemplate)
        {
            var postitionOfPlaceHolderClosingBracket = urlPathTemplate.IndexOf('}', counterForTemplate) + 1;

            var variableName = urlPathTemplate.Substring(counterForTemplate, postitionOfPlaceHolderClosingBracket - counterForTemplate);

            return variableName;
        }

        private int GetNextCounterPosition(string urlTemplate, int counterForTemplate, char delimiter)
        {
            var closingPlaceHolderPositionOnTemplate = urlTemplate.IndexOf(delimiter, counterForTemplate);
            return closingPlaceHolderPositionOnTemplate + 1;
        }

        private bool CharactersDontMatch(char characterOne, char characterTwo)
        {
            return char.ToLower(characterOne) != char.ToLower(characterTwo);
        }

        private bool ContinueScanningUrl(int counterForUrl, int urlLength)
        {
            return counterForUrl < urlLength;
        }

        private bool IsPlaceholder(char character)
        {
            return character == '{';
        }
    }
}
