# Sound Resources

This directory contains sound effect files for the Sudoku game.

## Required Sound Files

Place the following audio files in this directory:

1. **correct.mp3** - Played when a correct number is entered
2. **error.mp3** - Played when an incorrect number is entered or conflict detected
3. **complete.mp3** - Played when the puzzle is successfully completed
4. **hint.mp3** - Played when a hint is provided
5. **select.mp3** - Played when a cell is selected (optional)

## File Format

- Format: MP3 or WAV
- Recommended duration: 0.5-2 seconds for feedback sounds
- Sample rate: 44.1 kHz
- Bit rate: 128 kbps or higher

## Usage

The sound files are loaded and played using the Plugin.Maui.Audio package.
If a sound file is missing, the game will continue to function normally without audio.

## Free Sound Resources

You can find free sound effects at:
- freesound.org
- zapsplat.com
- soundbible.com
