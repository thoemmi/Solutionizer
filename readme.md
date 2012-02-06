# Solutionizer

Creating ad-hoc solutions for Visual Studio. It scans a desired directory for project files and allows the user to select from the 
projects to create a solution file. This solution can be saved or launched directly.


## TODO

### v0.1

- <strike>TFS support</strike>
- Metro theme
- Tooltips
- Settings
  - <strike>Persistence</strike>
  - UI
  - <strike>Saving and restoring of window size/position</strike>
- Separation of ViewModel for solution from the actual solution class
- Installer
- <strike>Removing of projects from the solution</strike>

### v.Next

- Folder history
- Caching of project files
- Solution folders
  - user defined
  - drag'n'drop
- <strike>add option to add referened projects as "unloaded"</strike> Not possible. "Loaded" state is persisted in proprietary *.suo files.