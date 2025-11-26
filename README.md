# UnsortedOrderer

.NET 8 console application for organizing a folder full of unsorted files. Configuration lives in `UnsortedOrderer.ConsoleApp/appsettings.json`.

## Configuration

- `SourceDirectory` — path to the folder you want to tidy up.
- `DestinationRoot` — root directory where sorted data will be moved.
- `SoftFolderName` — subfolder name for installers and software packages.
- `ArchiveFolderName` — subfolder name for archives.
- `ImagesFolderName` — subfolder name for icons, logos, and other small images.
- `PhotosFolderName` — subfolder name for large photos; within it, the app creates year and month subdirectories based on photo capture data.
- `RepositoriesFolderName` — subfolder name for repositories and source folders.
- `FirmwareFolderName` — subfolder name for firmware files (`.bin`, `.dat`).
- `MetadataFolderName` — subfolder name for metadata and sidecar files (for example, `.hprj`, `.xmp`).
- `EBooksFolderName` — subfolder name for e-books and comics.

## Sorting logic

- Large photos (including RAW) are moved to `PhotosFolderName` with a year/month breakdown determined from EXIF or file creation date. If EXIF dates are earlier than 1980, the file creation date is used instead.
- Small images (icons, previews, logos) go to the `images` subfolder inside `ImagesFolderName` without date breakdown. Size is determined by resolution (up to 512 px on the longer side) and file size (up to ~300 KB for small images).
- Documents, videos, 3D models, archives, digital certificates, firmware files, metadata, and software are sent to dedicated folders.
- E-books (`.epub`, `.fb2`, `.mobi`, `.azw3`, `.djvu`, `.cbr`, `.cbz`, `.ibooks`, `.kfx`) move to the `EBooksFolderName` folder.
- Video files go to `Videos` with automatic grouping by year, and within that by camera/phone make (`DJI Action`, `GoPro`, `Sony`, `Canon`, `Nikon`, `Ricoh`, `iPhone`, `Samsung`, `Google Pixel`, `Xiaomi`, `Realme`, `Nokia`, `Huawei`, `OnePlus`, `Motorola`, `Oppo`, `Vivo`, `LG`, `Asus`, `ZTE`) based on filename patterns. If metadata dates resolve to earlier than 1980, the file creation date is used to determine the year.
- If installer names (archives, `exe`, `msi`) differ only by version, files are gathered into a shared folder named after the program within the destination directory.
- Disk images (`.iso`, `.cso`, `.dmg`, `.img`, `.mdf`, `.mds`, `.nrg`, `.ccd`, `.isz`, `.vhd`, `.vhdx`) are placed in the `_Disk images` subfolder inside the software directory.
- Repositories and source folders are moved whole into `RepositoriesFolderName` without per-file sorting. Detection reacts to `.git`, common manifests (`package.json`, `pyproject.toml`, `go.mod`, `Cargo.toml`, etc.), or the presence of several source files (`.cs`, `.ts`, `.js`, `.py`, `.java`, `.cpp`, `.rs`, and more).
- The list of extensions to delete is defined in `appsettings.json` (by default `.lnk`, `.torrent`, `.tmp`, `.ini`).
- Installer directories are detected separately and moved intact into the software folder without unpacking.
- Device firmware directories (with documentation and helper files) are moved intact into `FirmwareFolderName`.
- When an archive name matches an already moved software folder, the folder is removed and the archive is placed in its spot.
- Empty directories are removed after processing.

Run the app from the repository root with `dotnet run --project UnsortedOrderer.ConsoleApp/UnsortedOrderer.ConsoleApp.csproj` after verifying paths in `appsettings.json` are correct.
