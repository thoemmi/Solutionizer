# Solutionizer

Creating ad-hoc solutions for Visual Studio. It scans a desired directory for project files and allows the user to select from the 
projects to create a solution file. This solution can be saved or launched directly.


## TODO

### v0.1

- <strike>TFS support</strike> ![Check](check.png)
- <strike>Metro theme</strike> ![Check](check.png)
- Tooltips
- Settings
  - <strike>Persistence</strike> ![Check](check.png)
  - <strike>UI</strike> ![Check](check.png)
  - <strike>Saving and restoring of window size/position</strike> ![Check](check.png)
- Separation of ViewModel for solution from the actual solution class
- Installer
- <strike>Removing of projects from the solution</strike> ![Check](check.png)
- <strike>Doubleclick on projects adds them to solution</strike> ![Check](check.png)
- <strike>Add current root path to window title</strike> ![Check](check.png)

### v.Next

- Folder history
  - add recent folders to jump list
- Caching of project files
- Solution folders
  - user defined
  - drag'n'drop
- <strike>add option to add referened projects as "unloaded"</strike> Not possible. "Loaded" state is persisted in proprietary *.suo files.
- Manual refresh of project repository
- Including *.sln in project repository
- multi-select tree view both in project repository and solution