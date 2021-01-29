fo-dicom for Unity
==================

Copyright (c) 2012-2017 fo-dicom contributors; Unity specific implementations (c) 2016-2017 Anders Gustafsson, Cureos AB
Licensed under Microsoft Public License, MS-PL, except Unity specific implementations.
All rights reserved.


Installation
------------
fo-dicom for Unity is provided as a single assembly DLL with an associated set of codec assemblies. fo-dicom is built against Unity version 4.5.0 (.NET 3.5), 5.6.0 (WSA/UWP) or 2017.1 (.NET 4.6). The DLL is dependent upon UnityEngine.dll.

To install fo-dicom for Unity, open the Unity Asset Store and search for fo-dicom. Click on the Buy button. After completing the purchase the Import Unity Package is displayed. Select the appropriate files and click Import to include fo-dicom in your project.

Please note that the dictionary XML files need to be located in a Resources folder and the DLLs (fo-dicom and codecs) need to be located in a Plugins folder. For WSA/UWP (Unity 5.6.0 and higher), there are separate DLLs that must be located in the Plugins/WSA sub-folder.

When using the .NET 4.6 version of fo-dicom, ensure that the Scripting Runtime Version in the Player Settings is set to .NET 4.6 Equivalent.


Usage
-----
fo-dicom for Unity is a scaled-down release of fo-dicom with limited compressed codec support. The .NET 3.5 based version of fo-dicom also does not support networking or asynchronous calls. The WSA/UWP version does not support networking.

The following compressed codecs are supported: JPEG Process 1, JPEG2000, JPEG-LS (decoding only), RLE

Unity specific image rendering is available. DICOM dictionary is synchronized with DICOM version 2017c.

Code examples:

    // Load DICOM object from file (loading from Stream is also supported)
    var file = DicomFile.Open(@"test.dcm");

    // Get data from file
    var patientid = file.Dataset.Get<string>(DicomTag.PatientID);
    var beamSequence = file.Dataset.Get<DicomSequence>(DicomTag.BeamSequence);
    foreach (var item in beamSequence.Items) {
        var beamNumber = item.Get<int>(DicomTag.BeamNumber);
    }

    // Add elements to existing DICOM object
    file.Dataset.Add(DicomTag.PatientsName, "DOE^JOHN");
    beamSequence.Items[2].Add(DicomTag.CompensatorTransmissionData, 0.1, 0.2, 0.3, 0.2, 0.3, 0.4, 0.3, 0.4, 0.5);

    // Create a new instance of DicomFile with different transfer syntax
    var newFile = file.ChangeTransferSyntax(DicomTransferSyntax.RLELossless);

    // Save updated file
    newFile.Save(@"output.dcm");

    // Render Image to Texture2D
    var image = new DicomImage(@"test.dcm");
    var texture = image.RenderImage().AsTexture2D();

    // Forward logging messages to Console:
    LogManager.SetImplementation(ConsoleLogManager.Instance);
	
    // Switch off logging messages
    LogManager.SetImplementation(null);
	
Additional usage information is available from the fo-dicom Github site, here: https://github.com/fo-dicom/fo-dicom

API documentation is available here: https://fo-dicom.github.io


Notes
-----
* The WSA/UWP version of fo-dicom currently does not build with UWP SDK 10.0.15063. Build against SDK 10.0.14393 instead.


Support
-------
E-mail to: support@cureos.com
