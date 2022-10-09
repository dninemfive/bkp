# bkp
![version](https://img.shields.io/github/v/release/dninemfive/bkp?color=brightgreen&label=Version) 
[![license](https://img.shields.io/badge/License-All%20rights%20reserved-blue.svg)](https://github.com/dninemfive/bkp/blob/master/LICENSE)
[![githubdls](https://img.shields.io/github/downloads/dninemfive/d9framework/total?color=blue&label=Github&logo=github)](https://github.com/dninemfive/bkp/releases/latest)

Backup utility which copies files to a specified directory, renaming them to their file hashes to avoid duplicating files, and preserving version history by saving directory structures. Semi-working, but not entirely polished at the moment:

- [x] Automated saving from specified folders to specified target folder
- [x] Indexing for deduplication
- [ ] Multithreading for performance
- [ ] Unpacking saved files
- [ ] `.bkpz` filetype to zip target directories and further reduce size