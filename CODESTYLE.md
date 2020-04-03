# Coding style

Sensate IoT is free, open source software and is currently maintained and
written by hobbyists. All help to make Sensate IoT a better system is welcome
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
if(condition) {
    yo_bro();
}
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
statement and the closing brace on the next empty line. The only exceptions
to this rule are:

* Type definitions;
* Namespaces;
* Functions.

These exeptions have both brances on the next empty line.

Class + method example:

```C#
namespace SensateService
{
    public class Example
    {
        private readonly string m_obj;

        public Example(string obj)
        {
            this.m_obj = obj;
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

Always place braces, even if they are not necessary:

```C#
if(condition is true) {
    DoThis();
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
existing code and you want it to stick it into the main stream Sensate IoT
repository. There's a few options you have:

* Sending a pull request via GitHub;
* Sensing a pull request via email;
* Sending a patch via email.

When sending a pull request via email, please specify:

* a link to your git repository
* a specification of the branch you want mainstream
* a summary of what you've been doing

### Creating pull requests

An explanation on how you should create your pull request. First of all,
make sure you adhere to all the style rules. When you think you do, you
can create your GitHub pull request. When you do so please keep the
following in mind:

* Make sure the pull request is able to merge;
* Make sure to fill out the entire pull request template;
* Assign labels, reviewrs, and milestones when able.

Make sure to keep an eye on your pull request after you have opened it. Failing
to reply to comments will probably get your pull request closed.
