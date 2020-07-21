Overview
========

This document summarises the build and release process for the project. The build scripts are written using `Cake <http://cakebuild.net/>`_, and are defined in `./build.cake`. The scripts have been designed to be run by either developers locally or by a build server (currently `CircleCi <https://circleci.com//>`_), with minimal logic defined in the build server itself.