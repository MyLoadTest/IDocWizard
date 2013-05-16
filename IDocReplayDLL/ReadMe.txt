The functions in this DLL are intended to be called from a VuGen script.
They are used for generating IDoc files, and managing the associated data.

Exported Functions:
  - idoc_set_license
  - idoc_select_input_file
  - idoc_eval_string
  - idoc_count_element
  - idoc_create
  - idoc_free_memory

NOTES:
- For functions to be called from a VuGen script, they must be exported as C functions (rather
  than C++). This is done by declaring functions as: extern "C" __declspec(dllexport)
- Each LoadRunner virtual user has its own memory space (even when running in "vuser per thread" mode).
  Even if a variable is global, its value cannot be accessed by other virtual users.
- LoadRunner functions can be called from within the DLL providing that lrun.h is #included and lrun50.dll and
  lrun50.lib are referenced by the Visual Studio Project.
- Documentation for LoadRunner functions can be found by opening a script in VuGen and pressing F1 for Help.


=======================================================================================================================

idoc_set_license(...)
~~~~~~~~~~~~~~~~~~~~~

The idoc_set_license() function must be called before the idoc_create() function will work.

This function expects a char* argument containing the license key XML:
     "<license>"
     "    <name>Joe User</name>"
     "    <company>BigCo</company>"
     "    <email>joe.user@example.com</email>"
     "    <key>ANUAA-ADHHB-BS7VU-MVH45-9ZG3B-U3PUQ</key>"
     "    <expires>2013-10-01</expires>"
     "</license>"

The license key XML is checked, and the license_valid global variable is set to TRUE if the license is valid.

=======================================================================================================================

idoc_select_input_file(...)
~~~~~~~~~~~~~~~~~~~~~~~~~~~

Selects the XML input file to be used by idoc_eval_string()

=======================================================================================================================

idoc_eval_string(...)
~~~~~~~~~~~~~~~~~~~~~

This function replaces an IDoc {parameter} in a string with a value extracted from the idoc_param_input_file XML.
The parameter will be in the format {IDoc:Segment:Field}
  - "IDoc" - is a constant string that identifies it as an IDoc parameter (as opposed to a standard LoadRunner
             parameter)
  - Segment - is the name of the Segment from the XML
  - Field - is the name of the Field within the specified Segment.

This function should behave the same way as the lr_eval_string() function (see LoadRunner documentation).
e.g. if the input file contained the following XML:
<ZISUPOD_ZBAPIPUBLISH02>
  <IDOC BEGIN="1">
    <EDI_DC40_U SEGMENT="1">
      <TABNAM>EDI_DC40_U</TABNAM>
      <MANDT>100</MANDT>
    </EDI_DC40_U>
  </IDOC>
</ZISUPOD_ZBAPIPUBLISH02>
... and the VuGen script contained this function call:
idoc_eval_string("abcdef{IDoc:EDI_DC40_U:MANDT}ghijklmnop");
Then the function should return "abcdef100ghijklmnop".

=======================================================================================================================

idoc_count_element(...)
~~~~~~~~~~~~~~~~~~~~~~~

Counts how many times the specified XML element appears in idoc_param_input_file
Note that the element can be a Segment or a Field (or any element in the IDoc).

=======================================================================================================================

idoc_create(...)
~~~~~~~~~~~~~~~~

This function builds an flat-text IDoc from a format specified in XML.
XML values can be {parameterized}.
See example files for function arguments (Action.c) and output format (idoc1.txt)

=======================================================================================================================

NOTES
Extern "C" means that the function name has been exported in "C style" (function name must be unique); "name
mangling" occurs in "C++ style" since C++ has overloading of function names so they are not unique.
http://stackoverflow.com/questions/1041866/in-c-source-what-is-the-effect-of-extern-c
Note that you will get an access violation error if you try to do anything with the char* that is returned by this
function. The area of memory it points to goes out of scope as soon as the function exits/returns.

This method signature is a bit of a WTF.
From: http://stackoverflow.com/questions/2442239/returning-char-in-function
When you declare pie as buffer in the function, you are not allocating heap memory, the variable is being created in
the stack. That memory content is only guaranteed within the scope of that function. Once you leave the function
(after the return) that memory can be reused for anything and you could find that memory address you are pointing at
overwritten at any time. Thus, you are being warned that you are returning a pointer to memory that is not guaranteed
to stick around.
If you want to allocate persistent memory in a c function that you can reference outside that function, you need to
use malloc (or other flavors of heap memory allocation functions). That will allocate the memory for that variable on
the heap and it will be persistent until the memory is freed using the free function. If you aren't clear on stack vs
heap memory, you may want to google up on it, it will make your C experience a lot smoother.

Choosing RapidXML (why?)
http://stackoverflow.com/questions/9387610/what-xml-parser-should-i-use-in-c

Read a file
http://stackoverflow.com/questions/2912520/read-file-contents-into-a-string-in-c

Moving a file with locking.

wrapper function for varargs function? -> easiest to write a macro instead.

TODO: function
- idoc_save (saves IDoc in a special directory according to its type and number)
