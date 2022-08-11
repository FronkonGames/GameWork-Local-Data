GAME:WORK LOCAL DATA MODULE
===========================

**Table of contents**

- `Introduction`_
- `Requirements`_
- `Installation`_
    * `Editing your 'manifest.json'`_
    * `Git`_
- `License`_

Introduction
------------

'**Local Data**' is a module of '**Game:Work**' dedicated to read and write local files asynchronously. It has these
features:

🔀 Fully asynchronous read, load and cancel.
🧬 Integrity check using MD5, SHA-1, SHA-256 or SHA-512 algorithms.
🗜️ Compression / decompression using algorithms: GZip, Zip or Brotli.
🔒 Encryption / decryption using algorithms: AES, DES, RC2, DES or TripleDES.
👌 Supports typical Unity data such as: Vector, Quaternion, Color, etc.

Requirements
------------

- Unity 2020.3 or higher.
- [Game:Work Core](https://github.com/FronkonGames/GameWork-Core).
- [Game:Work Foundation](https://github.com/FronkonGames/GameWork-Foundation).
- Test Framework 1.1.31 or higher.

Installation
------------

Editing your 'manifest.json'
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- Open the manifest.json file of your Unity project.
- In the section "dependencies" add:

```c#
{
  ...
  "dependencies":
  {
    ...
    "FronkonGames.GameWork.Modules.LocalData": "git+https://github.com/FronkonGames/GameWork-Local-Data.git",
    "FronkonGames.GameWork.Core": "git+https://github.com/FronkonGames/GameWork-Core.git",
    "FronkonGames.GameWork.Foundation": "git+https://github.com/FronkonGames/GameWork-Foundation.git"
  }
  ...
}
```

Git
^^^

First clone the dependencies inside your Assets folder:

```
git clone https://github.com/FronkonGames/GameWork-Foundation.git

git clone https://github.com/FronkonGames/GameWork-Core.git
```

Then clone the repository:

```
git clone https://github.com/FronkonGames/GameWork-Local-Data.git
```


License
-------

`MIT <LICENSE>`_