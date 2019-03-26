After a lot of looking around and trial and error, I adjusted the Virtualizing Wrap Panel of https://github.com/SonyWWS/ATF/wiki/ATF-and-WPF-Overview into something that covers my needs.

This is a virtualizing wrap panel, aimed to be used to present items from top to bottom and then from left to right, much like the List View of windows explorer.

It is meant to be used with items of equal height, as all my tests are based on this fact.

Furthermore, inside the project you will find a drag & multiselect option, one as a Dependency property and one as events, filtering, finding, adding and removing items from the list.
