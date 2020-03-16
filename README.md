# KeepFit

A Kerbal In-mission Fitness Degradation and Gee Loading Addon

## Features

All Kerbals have a fitness level.  Each capsule has a defined 'activity level' describing the comfort, room, and built-in fitness capabilities for crew in flight.  All Kerbal fitness levels go up or down based on their opportunities for exercise (which functions in the background, not just on the active vessel).  A living space can be cramped (losing 5% fitness per day), comfy (-1%/day), neutral (0%/day), or exercising (+1%/day).  Unfortunately the stock game does not have any parts that are suitable for exercising, but Kerbals will also exercise while they are at the Space Center or are landed on a planet with at least 0.05 G's of gravity.  You can track your astronauts' fitness in a "Truly Ugly" roster.

Fitness affects a Kerbal's tolerance for gee-loading, as does the duration of exposure.  A 100% fit Kerbal can tolerate up to 40 G's for one second, but only 20 G's for up to five seconds, 15 G's for one minute, and 10 G's for five minutes.  Their tolerance degrades to 50% of those values at zero fitness.  If you're not up for the risk of killing Kerbals, you can change a setting so the game will just gripe at you if your crew exceeds any of those thresholds.

By default, KeepFit assumes that a vessel is cramped (unless it's landed on a world with sufficient gravity), but you can write a Module Manager patch to add this module to any crew part to change that:

```
// slightly better
@PART[mk2LanderCabin]
{
	MODULE
	{
		name = KeepFitPartModule
		strActivityLevel = COMFY
	}
}

// even more palatable
@PART[cupola]
{
	MODULE
	{
		name = KeepFitPartModule
		strActivityLevel = NEUTRAL
	}
}
```

You can also specify that a capsule is ```CRAMPED``` or ```EXERCISING```.

## Dependencies

- KeepFit depends on **[ModuleManager](https://forum.kerbalspaceprogram.com/index.php?/topic/50533-18x-19x-module-manager-413-november-30th-2019-right-to-ludicrous-speed/)** to add the KeepFitPartModule to parts.

## Recommended addons

- **[Connected Living Spaces](https://forum.kerbalspaceprogram.com/index.php?/topic/192130-191-connected-living-spaces-adopted-2003-2020-03-04/)** lets Kerbals take advantage of the best quarters in the space accessible to them.  (You can enable this option without CLS, but using CLS makes it feel a little less cheaty by requiring that there must be a "short-sleeves" path to the exercise space.)

## Download and install

- **[GitHub](https://github.com/Kerbas-ad-astra/Timmers_KSP/releases)**

From there, just unzip the KeepFit folder into your GameData directory.

Please let me know in [**the forum thread**](https://forum.kerbalspaceprogram.com/index.php?/topic/192466-191-keepfit-refitted-kerbal-fitness-degradation-01300-02020-mar-16/) or on [**the GitHub issue tracker**](https://github.com/Kerbas-ad-astra/Timmers_KSP/issues) if you find any issues!

## Version history and changelog


- 0.11.9.8, 02016 Oct 30
	- Final release by timmers_uk, for KSP 1.2.0
- 0.12.4.5, 02017 Nov 2
	- Final release by Rodhern, for KSP 1.3.1
- 0.13.0.0, 02020 Mar 16
	- Recompiled for KSP 1.9.1 and CLS 2.0.0.3
	- Changed Kerbal icons so that their background colors reflects the text colors.
	- Internal logic reworked.  G-loading is now tracked as rolling averages which update once a second.

## Roadmap

There are some remnants of hooks into the (now-defunct) G-Effects mod that could stand to be removed.  There's also a lot of modern mod infrastructure that could be implemented (ToolbarController, ClickThroughBlocker, build automation, internationalization, and so on), and probably loads of other cruft that could stand to be removed.  I'm not inclined to be picky about pull requests.

Eventually, it might be cool to add some kind of space-limitation effect (e.g. only so many Kerbals can take advantage of an exercise space with a given number of seats) or the like, but at that point you're probably better off using a more involved mod, like [Kerbal Health](https://forum.kerbalspaceprogram.com/index.php?/topic/155313-181-kerbal-health-142-2020-02-11-massive-update-training-cmes-and-more/).

## Credits

Many thanks to timmers_uk and Rodhern for accepting my pull requests before, and huge thanks to timmers for letting me adopt it!

And also many thanks to micha for adopting CLS -- KeepFit just isn't the same without it!

## License

KeepFit (v0.13+) is copyright 2020 Kerbas_ad_astra and released under the MIT License.  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: 

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.