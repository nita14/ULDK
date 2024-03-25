# What is ULDK Client?

ArcGIS Pro Add-in to consume [ULDK API](https://uldk.gugik.gov.pl/?lang=en) (land parcel location service) which allows to identify and download land parcel data in Poland (for majority of counties). Service provided by GUGiK uder the CC BY 4.0 license.

## Requirements
- ArcGIS Pro Basic v. 3.2. License: Basic or higher.
- Access to the Internet. Whitelisted URLs:
  1. *.github.com
  2. *.uldk.gugik.gov.pl


## How to use ULDK Client?

1. Dowload the add-in. Check last release on [Github.com](https://github.com/nita14/ULDK)
2. Install add-in.
3. Open any ArcGIS Pro project (.aprx).
4. In the _Ribbon bar_ click on the _GUGiK_ tab.
5. Click _Show ULDK pane_.
6. Get parcel by:
   1. Sketching point, line, polyline. Spatial coverage limited to Poland.
   2. Select Commune, region and typing in a land parcel's unique identyfier.

The add-in is to add a land parcel as a graphic (shape and its id as text) and save it in a File Geodatabase located under _<project_path>/GUGIK/GUGIK.gdb/ULDK_ feature class.

## License
[CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/)

## Third-party tools/packages
1) Serilog (3.0.1)
2) Serilog.Sinks.File (5.0.0)
3) DotNetKit.Wpf.AutoCompleteComboBox (1.6.0)
4) Newtonsoft.Json (13.0.1)

## Bugs/enhancements
First, check the logs created in _<project_path>/GUGIK_ folder. Logs are created with the following naming convention: _ULDK_log_yyyy-MM-dd-T-HH-mm-ss_.

1) Add a new issue in the repo.
2) Send me an e-mail (adamnicinski[@]hotmail.com) with the description and zipped logs (as discribed above).

## Version history

- v. 1.0.0 - Initial release. ArcGIS Pro 3.2. March 2024. [Link](https://github.com/nita14/ULDK/releases/download/v1.0.0/ULDKClient.esriAddinX) 


