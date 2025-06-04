# Building LoL Reboot Setup

## Prerequisites
- Inno Setup Compiler

## Building Steps

### Prepare Release Build
1. Make sure you have built a Release version of the program first
2. Verify `bin\Release\net472\LoL Reboot.exe` exists

### Using Inno Setup Compiler
1. Open `SetupCompiler.iss` in Inno Setup Compiler
2. Press `F9` or select `Build > Compile`
3. The installer will be created in the output folder (defined in the script)

### Command Line Build
```
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" SetupCompiler.iss
```
(Adjust the path according to your Inno Setup installation)
