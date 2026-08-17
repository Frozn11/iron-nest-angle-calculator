# Iron Nest Angle Calculator

A console tool that calculates the firing (vertical) angle, minimum charges, and shell flight time needed to hit a target at a given distance, with optional horizontal angle tracking and a saved-shots list.

## Download

Prebuilt binaries are available on the [Releases page](https://github.com/Frozn11/iron-nest-angle-calculator/releases) for Windows, Linux, and macOS - both standalone (no .NET required) and framework-dependent (requires [.NET Runtime](https://dotnet.microsoft.com/download) installed) versions.

## How it works

Enter a target distance (in km) and the number of charges you want to use, and the tool calculates the vertical elevation angle to set on your gun, along with the shell's flight time. You can also optionally set a gun side and a horizontal angle, and every calculated shot is saved to a list for reference.

**Elevation formula:**

```
Elevation = (Distance in km × 12) / Charges
```

The result is rounded down to 2 decimal places.

Credit: formula sourced from the [Iron Nest subreddit](https://www.reddit.com/r/IronNest/comments/1vjgvbb/math/).

**Flight time formula:**

```
u = (Charges − 1) / 5
Adjusted Shell Speed = 0.7 × [0.3 + 0.7 × (3u² − 2u³)]
Flight Time = Distance / Adjusted Shell Speed
```

Credit: formula sourced from the [Iron Nest Wiki calculator](https://ironnestwiki.com/calculator).

## Minimum charges by distance

Each charge level has a minimum range it can effectively reach:

| Charges | Min Distance (km) |
|---------|--------------------|
| 1       | up to 5            |
| 2       | 5-10               |
| 3       | 10-15              |
| 4       | 15-20              |
| 5       | 20-25              |
| 6       | 25-30              |

The tool automatically calculates the minimum number of charges required for the distance you enter, and won't let you pick fewer than that. Distance is capped between 0.0005 km and 30 km.

## Usage

1. Run the program.
2. Select which gun you're aiming (`Left` / `L`, `Right` / `R`, or skip).
3. Optionally set a horizontal angle from 0.00 to 360.00 (or skip).
4. Enter the target distance in km (min 0.0005 km, max 30 km).
5. Enter the number of charges to use (must be between the calculated minimum and 6).
6. The tool prints the vertical angle, horizontal angle, charges used, and flight time, and saves the shot to the list.

**Example:**

```
Use /help for list of commands
Selcet Gun: Left(L) or Right(R) (can be skiped if not needed)
> L
Set horizontal angle from 0.00 to 360.00 (can be skiped if not needed)
> 90
Enter distance in km (min: 0.0005 km, max: 30.00 km):
> 12
Enter amout of charges (min: 3, max: 6)
> 3
----------------------------------------
  Left gun,
  vertical angle 48.00,
  horizontal angle 90.00,
  charges 3,
  time to travel 32.90 secondes

--------Saved-List--------
0.Left gun,
  vertical angle 48.00,
  horizontal angle 90.00,
  charges 3,
  time to travel 32.90 secondes
----------------------------------------
```

## Commands

Type any of these at an input prompt (starting with `/`) instead of a value:

| Command | Description |
|---------|-------------|
| `/help` | Shows the list of available commands |
| `/list` | Shows the list of saved shots |
| `/remove <index>` | Removes a saved shot by index (prompts for one if omitted) |
| `/savelist <true/false>` | Enables/disables saving new shots to the list (existing entries are kept) |
| `/alwaysshowlist <true/false>` | Toggles whether the saved list is printed after every shot |
| `/setmaxlist <number>` | Sets the maximum number of shots kept in the saved list; oldest entries are dropped once it's exceeded (default: 6) |

## Requirements

- .NET (any recent version supporting `System.Linq` and top-level console apps) - only needed if building from source or using a framework-dependent release build.

## Build & Run

```bash
dotnet build
dotnet run
```

## License

This project is licensed under the [MIT License](LICENSE).