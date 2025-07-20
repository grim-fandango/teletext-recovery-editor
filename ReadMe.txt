Teletext Recovery Editor

Please see the instruction manual: https://teletextarchaeologist.org/software/tre-documentation/

History
-------

Changes in 0.7.1.9
	- Feature: Application is now DPI aware
	- Feature: CTRL + SHIFT + UP/DOWN will now change the selected thumbnail
Changes in 0.7.1.8 (28th September 2024)
	- Feature: CTRL-N inserts the New Background attribute
	- Feature: CTRL-D inserts the Double Height attribute
Changes in 0.7.1.7 (26th November 2023)
	- When selecting a new carousel, the first page is now automatically opened
	- Bugfix: recoveries hashtable is now cleared when type is monolithic T42 and the carousel is changed
	- Bugfix: "Hex Position" box now moves with the other form elements on resizing

Changes in 0.7.1.0 (9th November 2023)
	- Now possible to choose the page from which the subtitles will be exported
	- A background image can be chosen from the local disk to be displayed when in Mix, Subtitles or Boxed modes

Changes in 0.7.0.2 (8th November 2023)
	- The program will now open single, large T42 files (typically containing a whole service)
	- Subtitles options added - export teletext subtitles as .SRT files
	- Import subtitles from an SRT file and output as .T42 teletext files
	- Mix mode added
	- Italian regional options character set added
	- Thumbnails window made larger to remove scrollbars
	- CTRL-L now replaces 0x00 with 0x20 because the Windows clipboard treats 0x00 as end-of-file.  This meant 
	  that only the line up until the 0x00 character was copied - should now be fixed, but of course the original
	  data is now different.
	- Menus and buttons now enabled and disabled depending on context
	- Program made less crashy


Changes in 0.6.1.2 (28th May 2022):
	- Thumbnails window is slightly bigger to avoid the horizontal slider

Changes in 0.6.1.0 (28th May 2022):
	- CTRL + cursor up and CTRL + cursor down now change the selected item in Recovered Pages 

Changes in 0.6.0.13 (15th May 2022):
	- Added a character to the "Hex Under Cursor" zone which shows the current character under the cursor (useful when in graphics mode);

Changes in 0.6.0.12 (13th May 2022):
	- Thumbnails no longer change size; charMap changes size instead
	- Thumbnail aspect ratio fixed

Changes in 0.6.0.11 (12th May 2022):
	- 42bp files are now t42s
	- Window is resizeable and thumbnails resize to the maximum Windows allows (256x256)
	- Borders removed on thumbnails to make them more readable

Chenges in 0.6.0.10 (2nd May 2022):
	- Alphanumeric colour attribute codes can now be entered using:
		Black	: CTRL-K
		Red		: CTRL-R
		Green	: CTRL-G
		Yellow	: CTRL-Y
		Blue	: CTRL-B
		Magenta : CTRL-M
		Cyan	: CTRL-S
		White	: CTRL-W

		Graphics codes are the same as the above but with holding SHIFT as well as CTRL.

	- Caps lock key now recognised

Changes in 0.6.0.9 (30th April 2022):
	- Added support for Level 2 three-phase flash, and level 2 normal flash
	- The Recovered Files list is now sorted on loading

Changes in 0.6.0.7 (13th April 2022):
	- Added Swedish/Finnish/Hungarian national options support

Changes in 0.6.0.6 ():
	- Limit on around 150 pages per carousel added to prevent out of memory exceptions

Changes in 0.6.0.5 (28th June 2021):
	- Hex Under Cursor now returns correct value when subpage is first clicked
	- Hex value for cursor with the parity bit now shown
	- Flash now working if enabled
	- Switching between double height and single height cursor now less laggy

Changes in version 0.6.0.4 (2nd June 2021)
	- Bug in the hex display gadget fixed
	- Folder browser changed to a more sane one

Changes in version 0.6.0.2 (9th March 2021)
	-Spanish/Portuguese national option characters added

Changes in version 0.6.0.0 (13th February 2021)
	- One level of undo added to the editor area
	- Text changes

Changes in version 0.5.1.0 (9th February 2021)
	- Issue with INS and DELs not being saved is fixed
	- Moving to another carousel after changes asks if you want to save and move, not save and move or cancel

Changes in version 0.5.0.0 (8th February 2021)
	- Fastext links now displayed (they are not editable)
	- Fixed issue with CTRL-C on a non-selected area

Changes in version 0.4.0.0 (8th February 2021)
	- if there is nothing selected, CTRL-C now selects whatever is under the cursor
	- Level 2.5 support improved, now supports borders
	- G3 glyphs now in the ETS typeface

Changes in version 0.3.0.0 (4th December 2020):
	- National Options character sets added for some countries (I've only implemented those which I have data for; if you need others, please ask)
	- Border mode is now switchable, defaults to on
	- All flags are now saved if you edit them
	- TTI export now finished
	- Various bugs, annoyances and irritations fixed (e.g. header is editable all of the time, hex value now shown correctly, plus many others)

Changes in version 0.2.0.0 (25th April 2020):
	- Added readme.txt which is displayed in this 'About' window
	- Added context menu options 'move to top' and 'move to bottom'
	- Added a service option, 'Trim and Combine into a T42' which, for each carousel, removes all subpages for a particular subpage apart from those 
		with the most packets, combines all carousels into a single T42 and saves
	- Bugfixes

Changes in version 0.1.0.0:

Features:

- Generic ASCII character set added - hand-drawn from the set used on This Is Ceefax
- 'Remove all but those with the most packets' attempts to do this with multi-page carousels as well as single page carousels.  Success depends on 
	how many packets are missing from the subpages.
	Manual work is usually necessary but it's an improvement, and we don't want it to get rid of any subpages that are in fact unique.
- Search menu item - does nothing yet
- Brought filetype, service type and row length management into the Service object
- Clicking on the current page on the recovered pages list refreshes the thumbnails if changes have been made

Bugs:
- Nasty bug fixed where pages would go missing from a carousel if you deleted a page from a carousel then moved them about
