# fourier-visualizer-amol
## Detailed descriprion of the program:
The purpose of this program is to create a desktop application containing 2D Plotter based on the idea of the Fourier Series. User can define list of circles with radiuses, and frequencies connected to each circle. First circle is anchored in the middle of plotter workspace. Each next circle center is anchored on previous one. After creating a list of circles user can start 10s procedure, in which each radius of each circle makes n rotations described by frequency field. For example:

    Circle 200, 1: a circle with a radius of 200 units, making one full clockwise rotation in 10 seconds
    Circle 80, -3: a circle with a radius of 80 units, making three full counter-clockwise rotations in 10 seconds

In the time of the procedure each radius of each circle is rotating from zero position proper number of times. Positions of circles have to be calculated dynamically for each step of the procedure. 

Window starts maximized and cannot be resized or maximized. When "Draw circles" option is turned on, added circles appear on the screen. All menu files are controllable by ALT+Key shortcuts.

* File button:
  * New: Clears list, clears image and stops procedure.
  * Open: Opens .xml serialized file with circles.
  * Save: Saves .xml serialized file with circles. 
* Exit: opens dialog with possible exit procedure.
* Options button:
  * Draw circles: Toggles drawing of helper circles.
  * Draw lines: Toggles drawing of helper lines.
  
The program draws and animates circles and their radii defined by the datagrid in the upper-left corner. It is also possible to (de)serialize .xml file with list of circles inside it. XML file is protected against possible errors.
