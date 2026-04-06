# Localized Magnifier
Localized Zoom feature in C# using WPF

This is a small desktop app that I've built in Windows Presentation Foundation (WPF).

![Magnifier Demo](MagnifierAppDemo.gif)

The idea behind the application was to have a lightweight zoom feature for reading small text while not having to 
change my display's resolution/scaling. The application captures a region near the cursor in a floating lens that
zooms the captured image, follows the mouse, and stays offset from the cursor.

## Current Features
- Circular magnifier overlay
- Follows cursor in real time
- Basic zoom & capture settings (hardcoded for now)
- Toggle on/off with __Ctrl + M__

## Notes
The core functionality is implemented. I'm currently focused on improving usability and polishing the application. 
I'd like this to be exportable and user-friendly for non-technical users at its end-stage.

Planned improvements:
- Settings GUI (adjust zoom, offsets, magnification, capture size, output size, change hotkey)
- Additional hotkey functionality (scroll wheel zoom)
- Visual polish (style with border and shadowing)
- Export as executable with optional startup/background support

## Goals
This project was built as a portfolio piece that also helps me learn about desktop apps,
rendering, OS-level interaction, and C#/.NET development. 
