# Welcome to the Jam

Welcome to the Extra Credits Game Jam everyone! Before anything else, I want
to thank you for so generously giving your time to this project. I am extremely
grateful that you would agree to donate so much of your time and talent for a project
whose only real payoff is fun and the opportunity to try something new!

Below, you will find instructions for setting up your environment so that we are all on
the same page regarding tools and workflow from the get-go!

## Create Accounts

Please register accounts with the following websites:

- <https://itch.io/>
- <https://github.com>
- <https://discordapp.com/>

## Join Teams

- Please join the Game Jam on `Itch.io` <https://itch.io/jam/extra-credits-game-design-jam>
- Please join the public server on `Discord` <https://discordapp.com/invite/SbWJAcN>
- Please join our team's server on `Discord` <https://discord.gg/sPxNXk>
  - Wait, do we really need our own server?! What's the point of IM when we are all in the
    same physical room? Primarily - this will be useful if we want to quickly share files
    and links with eachother, without having to go through GitHub.
- Please send me your `GitHub` username so I can add you as a collaborator on the project.

## Install Software

We will be using the following software for the project:

- [Git](https://git-scm.com/downloads) - version control system (see below)
- [GitHub Desktop (optional)](https://desktop.github.com/) - graphical UI for Git
  (if not comfortable with command line)
- [Unity](https://store.unity.com/download?ref=personal) - game engine (see below)
- [JetBrains Rider EAP (programmers only)](https://www.jetbrains.com/rider/eap/) - IDE
  for C# development
- [Adobe Photoshop](https://www.adobe.com/ca/products/photoshop.html) - I assume we all
  have this one already
- [Bosca Ceoil](https://boscaceoil.net/) - Free open-source midi-based song editor for
  quickly creating game and chiptune music.

## Setting Up Git

We will be using `git` for version control (program that tracks file changes and allows
for synchronous collaborative project editing). Please ensure that you have `git` installed
on your computer.

- [Download Git](https://git-scm.com/downloads)

Git is a command-line utility (On Mac OS, open `Terminal`, on Windows, open `Command Prompt`)
but there are also UI-based clients available.  If you are comfortable using the command line,
I highly reccomend it over any GUI tools, however if you want to use a UI, the best bet would
be `GitHub Desktop`.

- [Download GitHub Desktop (optional)](https://desktop.github.com/)
- [User Guide](https://help.github.com/desktop/)

If you are not familiar with using `git`, please take some time to learn the basics - it will
make things a lot easier! Here is a pretty good [no-frills basics guide](http://rogerdudler.github.io/git-guide/)

Finally, the project is hosted... right here! On GitHub. You are on the remote repository
right now. To clone this project on your machine from the command line:

```bash
git clone git@github.com:jhbertra/extra-credits-game-jam.git
```

From the `GitHub Desktop` GUI:

1. Sign in with your GitHub username / password
2. Click `Clone a repository` under `GitHub.com` select `jhbertra/extra-credits-game-jam`
3. Select the folder you want the repository to be cloned into (default is probablly fine).
4. Click `Clone`
5. Click `Initialize Git LFS` when prompted (large file storage used here to improve performance
   when storing Game asset files).

## Installing Unity

Unity is a popular game engine with a generous free tier license. Please install the latest
version of `Unity` (`2018.2.3`)

- [Download Unity Personal version](https://store.unity.com/download?ref=personal)

You can then open the project in Unity by clicking `Open` and navigating to the folder created
when checking out the `git` repository (Unity projects are folders).

## Setting up a Development Environment (Required for Programmers Only)

The preferred IDE for development is [JetBrains Rider](https://www.jetbrains.com/rider/eap/). The
repo has editor settings already configured for `ReSharper`, so this will make the collaboration go
a lot more smoothly, as this will be picked up by default. Please use the latest EAP release of `Rider`
as the `LTS` build is not 100% stable with Unity projects (ironically).  Please also [update Unity to
use Rider as the external code editor](https://answers.unity.com/questions/1240640/how-do-i-change-the-default-script-editor.html)
