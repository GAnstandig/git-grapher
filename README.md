![text](../media/images/title.png?raw=true)

Grapher is a command line tool used to create graphical representations of git commit histories.
The tool comes with several built-in color palettes, as well as a few external sample palettes.

Create a commit log by piping the output of `git log --all --date-order --pretty="%h|%p|"` to a file.

Create an image by running `GitGrapher.Interface.exe -f {path to commit log file}`

![text](../media/images/img1.png?raw=true)

By default, images are scaled up independently on the X and Y axes to preserve a minimum number of pixels between commits and the edges of the image. The scaling threshold can be configured independently on the X and Y axes by passing the proper parameters.

![text](../media/images/img3.png?raw=true)

An image title can be inserted in any of the four corners, or centered on the cardinal edges of the image. 
Title and background colors can be specified by providing RGBA values to the proper arguments
The image title's position can be fine-tuned by providing a pixel-level offset to the proper arguments

![text](../media/images/img2.png?raw=true)
