# Coding style

ETA/OS is free, open source software and is currently maintained and
written by hobbyists. All help to make ETA/OS a better system is welcome
ofcourse, but in order to keep things simple there are some regulations
or 'best practises'. Please read them carefully if you are planning to
send a patch.

This is probably the most important section of this entire document. Coding
style is VERRY important and patches with a bad coding style will be
rejected. If you want an example of a bad coding style; read the GNU coding
style (burn it after you're done, though).

## Indentation

Indentation is done using tabs, and tabs are 4 characters long. There are
some disgusting movements on the web trying to change standards to 2 characters
(*cough* hello Ruby *cough*). All this nonsense should be
ignored. <b>Patches not using 4-character indentation will be rejected.</b>

The main argument of people using 2 spaces per tab is that idents of 4 or 8 characters
move your code past the 100 character limit way to fast. This is, obviously,
a nonsense argument. The problem is not the indentation width, but the fact that
you have to many nested identations. Stop producing _WTF code_.

## General rules

Don't ever put statements on a single line. Use

```C#
if(condition)
    yo_bro();
```

instead of

```C#
if(condition) yo_bro();
```

### Switch statements

Switch statements are somewhat special. Don't double indent the case
statements. A proper example of a switch statement

```C#
switch(letter) {
case 'a':
case 'A':
    puts("Right answer!\n");
    break;

default:
    puts("Wrong answer!\n");
    break;
}
```

### Long lines

Statements longer than ~100 columns will be broken into sensible chunks, unless
exceeding 100 columns significantly increases readability and does not hide
information. Descendants are always substantially shorter than the parent and
are placed substantially to the right. The same applies to function headers
with a long argument list. However, never break user-visible strings such as
printk messages, because that breaks the ability to grep for them.

### Braces and spaces

The next issue that always comes up when its about C/C++ coding style is
the placement of braces.

The general rule is, place the opening brace on the same line as the
statement and the closing brace on the next empty line. The only exception
to this rule are functions, where both braces are placed on an empty line.

Class + method example:

```C#
namespace SensateService
{
    public class Example
    {
        private readonly string _obj;

        public Example(string obj)
        {
            this._obj = obj;
        }

        public void DoPrint()
        {
            Console.WriteLine(this._obj);
        }
    }
}
```

Normal example:

```C#
if(condition is true) {
    do this;
    and that;
}
```

As said the closing brace is on the next empty line, except in the cases
where it is followed by a continuation of the same statement (i.e. do-while,
if-else).

```C#
if(condition is true) {
    DoThis();
    Assign = That;
} else if(another condition is true) {
    SupBrah();
} else {
    Exit();
}
```

Do not place braces when they are not necessary:

```C#
if(condition is true)
    DoThis();
```

The last rule doesn't apply when only 1 branch of a conditional statement is
a single line:

```C#
if(condition is true) {
    do_this;
    do_that;
} else {
    Otherwise();
}
```

### Comments

Comments are good, but there is also a danger of over-commenting.  NEVER
try to explain HOW your code works in a comment: it's much better to
write the code so that the _working_ is obvious, and it's a waste of
time to explain badly written code.

Generally, you want your comments to tell WHAT your code does, not HOW.
Also, try to avoid putting comments inside a function body: if the
function is so complex that you need to separately comment parts of it,
you should probably go back to chapter 6 for a while.  You can make
small comments to note or warn about something particularly clever (or
ugly), but try to avoid excess.  Instead, put the comments at the head
of the function, telling people what it does, and possibly WHY it does
it.

All public API functions should be documented according to the Doxygen
standard.

C99 style (// comment) comments are allowed, but only for single
line comments. All comments using multiple lines should use the ANSI style.

## Patches

Now then. Lets imagine that you have written some code or fixed up some
existing code and you want it to stick it into the main stream ETA/OS
repository. There's a few options you have, sending a patch using email
(preferred, and required with small patches) or sending a pull request.

Pull requests are only an option when you are submitting a large patch. In
this case, large is defined as 10 commits and more. A succesful pull request
contains:

* a link to your git repository
* a specification of the branch you want mainstream
* a summary of what you've been doing

All patches that are 1-9 commits are to be sent by email.

### Creating patches

An explanation on how you should create your patch patch. Lets say, that
your patch is 2 commits long and you want to generate the .patch files.

Enter the following command:

```bash
git format-patch --to etaos@googlegroups.net HEAD~2..HEAD
```

git format-patch will generate a patch-file-per-commit of the given range
(which is HEAD-2 to HEAD in our case). The --to argument is used to set
a receiver which will be used later on by git send-email.

@subsection snt-patch Sending patches
Once your patch has been created sent them <b>inline</b> to the ETA/OS
mailing list (etaos@googlegroups.net). If you want to you can also CC
the maintainer of the file you have patched, his or her email should
be in the top of the file.

To send your email using git, run the following command:

```bash
git send-email *.patch
```

To use git send-email, you need to have it configured. To do so, please
read the git scm documentation on `git send-email'.

Patches can also be manually sent using email clients. Please note that
patches should be sent as plain text. No MIME, links, compression or
attachments.

Exception to the rule: if your email client is mangling up your patch
a maintainer may ask you to resend it using MIME or attachments.

### Email clients

Below follow some email settings which make sure that your patch arrives
as a clean plain-text patch.

#### Evolution

Some people use this successfully for patches.

When composing mail select: Preformat
from Format->Heading->Preformatted (Ctrl-7) or the toolbar

Then use: Insert->Text File... (Alt-n x)
to insert the patch.

You can also "diff -Nru old.c new.c | xclip", select Preformat, then
paste with the middle button.

#### Mutt

This editor is most likely going to be the most succesful for sending
plain-text patches.

Mutt doesn't come with an editor, so whatever editor you use should be
used in a way that there are no automatic linebreaks.  Most editors have
an "insert file" option that inserts the contents of a file unaltered.

To use 'vim' with mutt:
set editor="vi"

If using xclip, type the command

* :set paste
* before middle button
* shift-insert
* use :r filename

If you want to include the patch inline. (a)ttach works fine without
"set paste".

Config options:
It should work with default settings. However, it's a good idea to set the
"send_charset" to: set send_charset="us-ascii:utf-8"

@subsubsection thunder Thunderbird
Thunderbird is an Outlook clone that likes to mangle text, but there are ways
to coerce it into behaving.

* Allows use of an external editor:
  The easiest thing to do with Thunderbird and patches is to use an
  "external editor" extension and then just use your favorite $EDITOR
  for reading/merging patches into the body text.  To do this, download
  and install the extension, then add a button for it using
  View->Toolbars->Customize... and finally just click on it when in the
  Compose dialog.

To beat some sense out of the internal editor, do this:

* Edit your Thunderbird config settings so that it won't use format=flowed.
  Go to "edit->preferences->advanced->config editor" to bring up the
  thunderbird's registry editor.

* Set "mailnews.send_plaintext_flowed" to "false"
* Set "mailnews.wraplength" from "72" to "0"
* "View" > "Message Body As" > "Plain Text"
* "View" > "Character Encoding" > "Unicode (UTF-8)"
