
# Essence Archive Viewer
![](/preview.png)
Supported `SGA` versions (based on `Essence.Core.dll`):
- `4` ompany of Heroes 1
- ~~`5` Dawn of War II~~
    - _Appears to be broken_
- `6` ??
- `7` Company of Heroes 2
- `8` ??
- `9` Dawn of War III
- `10` Company of Heroes 3, Age of Empires IV

This is a decompiled and altered version of the Relic Entertainment's official [Archive Viewer](http://modding.companyofheroes.com/archive-viewer). Rough overview of changes:
- Replaced referenced `RelicCore.dll` with `Essence.Core.dll`
   > _`Essence.Core.dll` is available in the first public pre-alpha build of Company of Heroes 3: `\steamapps\common\Company of Heroes 3 - Pre-Alpha Preview\dev\mstest\TestProjects\Essence.Rpc.FunctionalTests.Anvil\Essence.Core.dll`_
- Change all namespace references from `RelicCore.Archive` to `Essence.Core`
- Fix various issues preventing the compilation of the decompiled project

The library providing APIs (`Essence.Core.dll`) to read and extract contents of `SGA` files is untouched.

## Download
- See [releases](https://github.com/Janne252/essence-archive-viewer/releases)
