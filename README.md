# LightBar For Keyboard

LightBar For Keyboard is a lightweight, system-integrated Windows application designed to create a customizable light bar at the bottom of your screen. It serves as a software-based keyboard light, reflecting light from your monitor down onto your laptop or desktop keyboard in low-light environments.

## Features

- **Adjustable Height & Brightness Boost**:
  - **Height Presets**: Choose between Standard (40px Default), Medium Light (80px), High Light (120px), Ultra Light (160px), and Max Light (200px) directly from the context menu.
  - **Double-Click Quick Toggle**: Double-click anywhere on the light bar to instantly toggle between boost height and default size (40px).
  - **Mouse Wheel Resizing**: Hover over the light bar and scroll your mouse wheel up or down to smoothly and dynamically adjust the height.
  - **Revert to Default Size**: Instantly snap back to the baseline 40px height with a single click.
- **Color Profiles & OLED Optimization**: Choose from multiple colors optimized for contrast and energy savings:
  - **White**: Standard brightness.
  - **Pure Green**: Maximum contrast on dark keys.
  - **Pure Red**: Protects night vision and offers the lowest power draw.
  - **Yellow**: Warm, eye-friendly light.
- **Docked Mode (Default)**: Registers as a native Windows Application Desktop Toolbar (AppBar). It securely locks above the taskbar and pushes maximized windows upward so they never cover your light source. Dynamically resizes the desktop work area when height is increased.
- **Floating Mode**: Switch to Floating Mode to freely drag the bar anywhere vertically or horizontally.
- **Single Instance Engine**: Automatically prevents multiple overlapping bars from opening accidentally.
- **High DPI Support**: Fully optimized for Windows 11 display scaling, maintaining crisp rendering on any monitor setup.

## Energy Efficiency vs. Hardware Lighting

Why use software to light your keyboard instead of a physical illuminated keyboard or a USB lamp? The answer is energy efficiency and convenience.

**Hardware Consumption (Illuminated Keyboard / USB Lamp):**
A standard USB port supplies 5 V. Depending on the number and intensity of the LEDs, a physical light consumes constant extra power:

- Minimum (small USB lamp): 5 V * 0,1 A = **0,5 W**
- Maximum (RGB keyboard / bright lamp): 5 V * 0,5 A = **2,5 W**

**LightBar Software Consumption:**
Our application leverages the screen you are already using.

- **LCD Monitors (IPS, TN, VA):** The backlight is already on. Displaying a white or colored bar merely twists the liquid crystals to let existing light through. **Extra power required: 0 W.**
- **OLED Monitors:** Each pixel generates its own light. While a full white bar uses about **2,0 W**, we introduced pure color profiles to bypass subpixel activation and save battery:
  - **Pure Green:** ~0,5 W (75% savings)
  - **Pure Red:** ~0,3 W (85% savings)
  - **Yellow:** ~0,8 W (60% savings)

**The Verdict:** On standard LCD screens, LightBar costs literally zero extra energy, making it infinitely more efficient than hardware lighting. On OLEDs, pure color modes provide excellent contrast at a fraction of a watt, while protecting your night vision and eliminating cable clutter entirely.

## How to Use

1. **Launch**: Install the application using the setup file, or double-click the portable `LightBarForKeyboard.exe`.
2. **Adjust Height & Brightness**:
   - **Double-Click** the bar to toggle between Boost mode and Default size (40px).
   - **Scroll the Mouse Wheel** over the bar to increase or decrease height.
   - Right-click and choose **Height Presets** or **Revert to Default Size (40px)**.
3. **Change Color**: Right-click anywhere on the bar and select your preferred color profile.
4. **Move (Floating Mode)**: Right-click the bar, select **Floating Mode**, then hold down the **left mouse button** to drag it anywhere.
5. **Lock (Docked Mode)**: Right-click the bar and select **Docked Mode** to pin it back to the bottom edge.
6. **Close**: Right-click and choose **Exit**, or press `Alt + F4`.

## Download

You can download the latest Setup Installer or the portable standalone executable from the Releases section:
👉 **[Download LightBar For Keyboard Latest Release](https://github.com/MediaExpres/LightBarForKeyboard/releases)**

### Note on Windows SmartScreen
Because this is a new, independent application without a corporate Code Signing certificate, Windows Defender SmartScreen might display a blue "Windows protected your PC" warning. This is a standard security prompt for new software.

**To run the application:**
1. Click on **More info** in the blue popup.
2. Click the **Run anyway** button that appears.

## Support the Project

If you find LightBar For Keyboard useful and it helps you save energy and work better in the dark, consider supporting its development! This software is free, but your support helps keep the lights on (literally and figuratively). 

<a href="https://buymeacoffee.com/mihaipavel6">
  <img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" width="150">
</a>

**[👉 Sponsor via PayPal](https://paypal.me/MediaExpres)**

## License & Copyright

© 2026 MediaExpres. All rights reserved.

This software is **Free to use and modify** under the following conditions:

- **Attribution**: You must give appropriate credit to the original source.
- **Modification**: You are free to modify this code for your own needs.
- **No Warranty**: This software is provided "as-is", without warranty of any kind.
