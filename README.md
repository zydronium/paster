# paster

INTRODUCTION

The Paster.NET a 6core paster clone in C#.NET. It has just 1 feature:
it makes stuff you paste, reachable by a unique URL you can share with others. A
paste can be some test that you enter via the web GUI, or text/image that you
paste via curl.

The text you paste here will be reachable by a unique URL that is long enough to
prevent brute-forcing. Also, these identifiers are randomly chosen. You post
will remain on the server for at most 30 days.

USAGE

You can use Paster.NET by accessing the URL to your installation via a browser or
via a bash oneliner. The oneliner is used as follows to paste a file:

    ~$ 6p /etc/hosts

Paste from STDIN:

    ~$ echo "hi" | 6p

Paste from here document:
    ~$ cat << EOF | 6p

The oneliner for in your .bash_profile:

    6p() { curl -s -F "content=<${1--}" -F ttl=604800 -w "%{redirect_url}\n" -o
    /dev/null https://p.6core.net/; }

INSTALL

Copy the stuff in to the webroot.

Update web.config to reflect your settings.
