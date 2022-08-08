<p align="center"><img src="Documentation/Animation.gif" /></p>
<p align="center"><b>Async Load/Save Local Data With Compression, Encryption And Integrity Check</b></p>
<br>

<p align="center">
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/package-json/v/FronkonGames/GameWork-Local-Data?style=flat-square" alt="version" />
  </a>  
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/license/FronkonGames/GameWork-Local-Data?style=flat-square" alt="license" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/languages/top/FronkonGames/GameWork-Local-Data?style=flat-square" alt="top language" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/codacy/grade/f124de6116ed47099d017f8ae6011e26?style=flat-square" alt="code quality" />
  </a>
</p>

'**Local Data**' is a module of '**Game:Work**' dedicated to read and write local files asynchronously. It has these
features:

🔀 Fully asynchronous read, load and cancel.<br>
🧬 Integrity check using MD5, SHA-1, SHA-256 or SHA-512 algorithms.<br>
🗜️ Compression / decompression using algorithms: GZip, Zip or Brotli.<br>
🔒 Encryption / decryption using algorithms: AES, DES, RC2, DES or TripleDES.<br>
👌 Supports typical Unity data such as: Vector, Quaternion, Colour, etc.

## Requisites 🔧

- Unity 2020.3 or higher.
- [Game:Work Core](https://github.com/FronkonGames/GameWork-Core).
- [Game:Work Foundation](https://github.com/FronkonGames/GameWork-Foundation).
- Test Framework 1.1.31 or higher.

## Installation ⚙️

### Editing your 'manifest.json'

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

## Use 🚀

TODO.

## License 📜

Code released under [MIT License](https://github.com/FronkonGames/GameWork-Scene-Module/blob/main/LICENSE).