# Iron Nest Angle Calculator

A simple console tool that calculates the firing angle and minimum charges needed to hit a target at a given distance.

## Download

Prebuilt binaries are available on the [Releases page](https://github.com/Frozn11/iron-nest-angle-calculator/releases) for Windows, Linux, and macOS - both standalone (no .NET required) and framework-dependent (requires [.NET Runtime](https://dotnet.microsoft.com/download) installed) versions.

## How it works

Enter a target distance (in km) and the number of charges you want to use, and the tool calculates the elevation angle to set on your gun.

**Formula:**

```
Elevation = (Distance in km × 12) / Charges
```

Credit: formula sourced from the [Iron Nest subreddit](https://www.reddit.com/r/IronNest/comments/1vjgvbb/math/).

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

The tool automatically calculates the minimum number of charges required for the distance you enter, and won't let you pick fewer than that.

## Usage

1. Run the program.
2. Select which gun you're aiming (`Left` / `L`, `Right` / `R`, or skip).
3. Enter the target distance in km (minimum 0.5 km).
4. Enter the number of charges to use (must be between the calculated minimum and 6).
5. The tool prints the elevation angle to set and the charges used.

**Example:**

```
Selcet Gun: Left(L) or Right(R) (can be skiped if not needed)
> L
Enter distance in km (min: 0.5 km):
> 12
Enter amout of charges (min: 3, max: 6)
> 3
Left gun, set angle to 48, charges needed 3
```

## Requirements

- .NET (any recent version supporting `System.Linq` and top-level console apps) - only needed if building from source or using a framework-dependent release build.

## Build & Run

```bash
dotnet build
dotnet run
```

## License

This project is licensed under the [MIT License](LICENSE).
