# Contributing to Andromeda

If you want to contribute to Andromeda means that you're awesome!

Many thanks to [MaterialUI](https://material-ui.com/), this document was heavily inspired by their awesome documentation.

The following steps will help you to contribute on the right way and get your PR merged easily.

## Code of conduct

Andromeda has adopted the [Contributor Covenant](https://www.contributor-covenant.org/) as its Code of Conduct. We expect that our contributors be aware of it. Please read the [full text](./code_of_conduct.md) before starting to code to understand what actions will and will not be tolerated.

## Your First Pull Request

Andromeda is a community project, therefore, Pull Requests are always welcome. However, before working on a large change, it is best to open an issue first to discuss it with the maintainers.

If it is your first Pull Request, this series of videos are a great source to learn more about Pull Requests: [How to contribute to an open source project on github](https://egghead.io/courses/how-to-contribute-to-an-open-source-project-on-github)

Tips to increase the chance of being accepted:

* Keep your Pull Request small.
* Don't bundle more than one feature or bug fix per Pull Request. It's often better to create two smaller Pull Requests than one big one.

### How to send a Pull Request

1. Fork the repository.

2. Clone the fork to your local machine and add upstream remote:

```
git clone git@github.com:<yourname>/andromeda.git
cd material-ui
git remote add upstream git@github.com:mui-org/andromeda.git
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

Keep in mind that GitHub will automatically run the CI. Therefore, do a double check on the linters, check if you are not adding any credential file and if all the tests are passing.

Make sure you install the git pre-commit hook. For more details, see the ```hooks/``` directory.

## Thanks for help us to make Andromeda great!
