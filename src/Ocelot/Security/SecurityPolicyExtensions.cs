using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Security
{
    public static class SecurityPolicyExtensions
    {
        public static string GetClientIpAddress(this HttpContext httpContext, bool tryUseXForwardHeader = true)
        {

            string ip = null;
            if (httpContext == null)
            {
                return ip;
            }
            // X-Forwarded-For =>  Using the First entry in the list
            if (string.IsNullOrWhiteSpace(ip) && tryUseXForwardHeader)
            {
                ip = httpContext.GetHeaderValue("X-Forwarded-For").SplitCsv().FirstOrDefault();
            }
            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && httpContext.Connection?.RemoteIpAddress != null)
            {
                ip = httpContext.Connection.RemoteIpAddress.ToString();
            }
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.GetHeaderValue("REMOTE_ADDR");
            } 
            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }
            return ip;
        }
     


        public static string GetHeaderValue(this HttpContext httpContext, string headerName)
        {
            if (httpContext?.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
            {
                return values.ToString();
            }
            return string.Empty;
        }

        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
            {
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();
            }

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable()
                .Select(s => s.Trim())
                .ToList();
        }
    }
}
