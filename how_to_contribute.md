# Contributing to Andromeda

If you want to contribute to Andromeda means that you're awesome!

The following steps will help to contribute on the right way and get your PR merged easily.

## Code of conduct

Andromeda has adopted the [Contributor Covenant](https://www.contributor-covenant.org/) as its Code of Conduct. We expect that our contributors be aware of it. Please read the full text before start on code, it can be seen [here](./code_of_conduct.md).

## Your First Pull Request

Keep in mind that Andromeda is a community project. 
Pull Requests are always welcome, but, before working on a large change, it is best to open an issue first to discuss it with the maintainers.
This video series is recommended to understand [How to contribute to an open source project on github](https://egghead.io/courses/how-to-contribute-to-an-open-source-project-on-github)

Thanks to [MaterialUI](https://material-ui.com/) to its incredible docs, we followed they tips on this section

When in doubt, keep your Pull Requests small. To give a Pull Request the best chance of getting accepted, don't bundle more than one feature or bug fix per Pull Request. It's often best to create two smaller Pull Requests than one big one.

Fork the repository.

Clone the fork to your local machine and add upstream remote:

```
git clone git@github.com:<yourname>/material-ui.git
cd material-ui
git remote add upstream git@github.com:mui-org/material-ui.git
```

Synchronize your local master branch with the upstream one:

```
git checkout master
git pull upstream master
```

Double check if the environment is set and working fine, the instructions can be seen [here](./how_to_setup.md)

Create a new topic branch:

```
git checkout -b my-topic-branch
```

Make changes, commit and push to your fork:

```
git push -u
```

Go to the repository and make a Pull Request.
The core team is monitoring for Pull Requests. We will review your Pull Request and either merge it, request changes to it, or close it with an explanation.

## How to increase the chances on being accepted?

Keep in mind that GitHub will automatically run the CI.

Double check on linters, double check if you are not updating credentials and if the tests are passing.

## Thanks for help us to make Andromeda great!
