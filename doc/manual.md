# Checkouts Overview - Usage Manual
Overview dashboard app for source code repository checkouts.


## Setup
The Checkouts Overview app is provided as portable zip.
No installation is required.
You can grab the [latest release](https://github.com/sgrottel/checkouts-overview/releases), or even pre-release from successful CI runs.

The zip file contains all files within a directory named `Checkouts-Overview`.
Extract it to any location on your computer you like.
Shortcuts, e.g. to the Desktop or the Start menu, are not generated automatically. If you want those, you need to create them yourself.
Since all future releases can be extracted in the same place, overwriting the previous files, your shortcuts will stay valid.

Your application settings are automatically migrated from older versions to newer versions (not the other way round).


## List of Entries

TODO


### Entries

TODO


### Icons

TODO


### Load/Save

TODO


### Update

TODO


### Sort
Click on \[Sort...\] in the menu bar, and a second menu bar will open beneath, showing the sorting commands.

TODO


## Scanning Disks

TODO


### Everything

TODO


### File System Scan

TODO


## Settings
Click on \[Settings\] to open the applications settings dialog window. There you can configure:

* The `Last File` that was opened or saved by the application.
* Switch on automatic `Load last file on start`.
* Switch on automatic `Scan disks on start` (not recommended).
* Switch on automatic `Update entries on start`.
* Configure the path to the `Git` command line client.<br>
If empty, the default git command line client will be used, as configured by the system's `PATH` variable.
* Configure the possible names of `Git Main` branches.<br>
This is a semicolon separated list of names, e.g. "`main;master`".
If empty, the names `main` and `master` will be used.
* Configure the `UI Client` application to be started as `Client` from the main window.<br>
The client application will be called with the selected repositories path as first argument.


## Links
* https://github.com/sgrottel/checkouts-overview -- Project Website, hosting [Releases Downloads](https://github.com/sgrottel/checkouts-overview/releases), source code, documentation, and work log
* https://github.com/sgrottel/checkouts-overview/releases -- Releases and Downloads
* https://www.sgrottel.de -- Developer's website
* https://www.voidtools.com -- Home of the [Everything](https://www.voidtools.com/) file system utility
* https://git-scm.com/download/win -- GIT for Windows


## License
Copyright 2021 SGrottel (https://www.sgrottel.de)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

For details, see [LICENSE](../LICENSE) file.
