# Case Closed

Unity 2D detective game. See [the recording evidence map](docs/requirements-from-recordings.md) and [development rules](AGENTS.md).

## Prerequisites and setup
Install Unity Hub and Unity **6000.3.27f1**, plus the platform build module you intend to use. Add this repository root as an existing Unity project and open it. Unity restores packages from `Packages/manifest.json` and `Packages/packages-lock.json`; internet access may be needed on first import. No npm, database, API credentials, or environment variables are required.

## Run
Open `Assets/Scenes/MainMenu.unity` and press Play. Release held keys, then press a key or primary pointer button to open case selection. Back returns to the existing menu, where Settings and Quit remain available. Select an unlocked case to start its story introduction and investigation. A new Editor Play session shows the opening again.

## Build
Use Unity's File > Build Profiles, choose the installed target platform, and build with the existing enabled scenes in their authored order (MainMenu first). Do not run scene rebuilders to prepare a build. Unity's generated Library is not part of source control.

## Controls and verification
Follow the in-game controls for dialogue, evidence inspection, notebook, and conclusion. The opening supports keyboard and primary pointer input. Menu controls retain keyboard navigation and their existing confirmation behavior.

The project deliberately uses documented manual Play Mode checks rather than project unit-test suites. There is no npm test or test-runner command. Follow the checklist and recorded results in `docs/requirements-from-recordings.md`; confirm compilation and inspect Console errors without clearing history. Check both wide and 4:3 Game View layouts.
