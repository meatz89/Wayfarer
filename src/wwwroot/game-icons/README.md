# Game Icons

This directory contains SVG icons from [Game-Icons.net](https://game-icons.net).

## License

All icons are licensed under **Creative Commons Attribution 3.0 Unported (CC BY 3.0)**.

See: https://creativecommons.org/licenses/by/3.0/

## Attribution

Icons created by:
- **Lorc:** brain, crown, cut-diamond, drama-masks, magnifying-glass, scales, target-arrows
- **Delapouite:** alarm-clock, biceps, coins, meal, shaking-hands, sparkles
- **sbed:** health-normal
- **Skoll:** hearts

## Usage

Icons are loaded dynamically by the `Icon.razor` Blazor component:

```razor
<Icon Name="coins" CssClass="resource-coin" />
```

## Adding New Icons

1. Search for icon on https://game-icons.net
2. Download white SVG version (ffffff/000000)
3. Save to this directory as `{icon-name}.svg`
4. Document creator attribution in THIRD-PARTY-LICENSES.md
5. Use via `<Icon Name="{icon-name}" />`

## Modifications

Icons are styled via CSS at runtime (color, size). Original SVG files remain unmodified.
