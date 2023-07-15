# SDL2 base application in C#

* Uses SDL2 to create a surface and manage windows and input, but everything else is done through writing directly to a C# array that represents the RAM

## Installation

It is possible nothing additional is needed, NuGet is used to pull the SDL2-CS project. In case something is needed:

### MacOS

Install sdl2, using for example brew:

```
brew install sdl2
```

### Linux

Install sdl2, using for example apt-get:

```
apt install libsdl2-dev
```

## Usage

* Clone and use directly. Just need to change the function that is passed to the SDLApp.Run method to change the behaviour function.

## Licenses

Engine code developed by [Diogo de Andrade][DAndrade] and [Nuno Fachada][NFachada]; it is made available under the [Mozilla Public License 2.0][MPLv2].

Code uses:

* [SDL2#][SDL2#] by [Ethan Lee][ELee]

All the text and documentation (i.e., non-code files) are made available under
the [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International
License][CC BY-NC-SA 4.0].

[MPLv2]:https://opensource.org/licenses/MPL-2.0
[CC BY-NC-SA 4.0]:https://creativecommons.org/licenses/by-nc-sa/4.0/
[CC-BY3.0]:https://creativecommons.org/licenses/by/3.0/
[CC0]:https://creativecommons.org/publicdomain/zero/1.0/
[Ap2]:https://opensource.org/licenses/Apache-2.0
[SDL2#]:https://github.com/flibitijibibo/SDL2-CS/blob/master/LICENSE
[ELee]:https://github.com/flibitijibibo
[ImageSharp]:https://github.com/SixLabors/ImageSharp
[DAndrade]:https://github.com/DiogoDeAndrade
[NFachada]:https://github.com/fakenmc
