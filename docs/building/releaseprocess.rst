Release process
===============

* The release process works best with GitHubFlow branching. 
* Contributors can do whatever they want on PRs and merges to master will result in packages being released to GitHub and NuGet.

Ocelot uses the following process to accept work into the NuGet packages.

1. User creates an issue or picks up an existing issue in GitHub. 

2. User creates a fork and branches from this (unless a member of core team, they can just create a branch on the main repo) e.g. feat/xxx, fix/xxx etc. It doesn't really matter what the xxx is. It might make sense to use the issue number and maybe a short description. I don't care as long as it has (feat, fix, refactor)/xxx :) 

3. When the user is happy with their work they can create a pull request against master in GitHub with their changes. The user must follow the `SemVer <https://semver.org/>`_ support for this is provided by `GitVersion <https://gitversion.readthedocs.io/en/latest/>`_. So if you need to make breaking changes please make sure you use the correct commit message so GitVersion uses the correct semver tags. Do not manually tag the Ocelot repo this will break things.

4. The Ocelot team will review the PR and if all is good merge it, else they will suggest feedback that the user will need to act on. In order to speed up getting a PR the user should think about the following.
    - Have I covered all my changes with tests at unit and acceptance level?
    - Have I updated any documentation that my changes may have affected?
    - Does my feature make sense, have I checked all of Ocelot's other features to make sure it doesn't already exist?
In order for a PR to be merged the following must have occured.
    - All new code is covered by unit tests.
    - All new code has at least 1 acceptance test covering the happy path.
    - Tests must have passed.
    - Build must not have slowed down dramatically.
    - The main Ocelot package must not have taken on any non MS dependencies.

5. After the PR is merged to master the release process will begin which builds the code, versions it, pushes artifacts to GitHub and NuGet packages to NuGet.

6. The final step is to go back to GitHub and close any issues that are now fixed. You should see something like this in`GitHub <https://github.com/ThreeMammals/Ocelot/releases/tag/13.0.0>`_ and this in `NuGet <https://www.nuget.org/packages/Ocelot/13.0.0>`_.

Notes
-----

All NuGet package builds & releases are done with CircleCI `here <https://circleci.com/gh/ThreeMammals>_` and all releases are done from `here <https://ci.appveyor.com/project/TomPallister/ocelot-ayj4w>_`.

Only TomPallister can merge releases into master at the moment. This is to ensure there is a final quality gate in place. Tom is mainly looking for security issues on the final merge.
