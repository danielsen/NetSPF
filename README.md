# NetSPF

`NetSPF` is a SPF verification utility for .NET.

### Packages

- Current Version: `0.1.0`
- Target Framework: `.NET Standard 2.0`

### Dependencies

- [Resolution](https://www.nuget.org/packages/Resolution/)

### Usage

In keeping with convention the `SpfResolver` class provides a function called
`CheckHost` as an entry point to verification. `CheckHost` takes the following
parameters: 

- IPAddress, (IPv4 or IPv6) the IP emitting the mail.
- domainName, the domain label from MAIL FROM or HELO.
- sender, the MAIL FROM or HELO identity, e.g. noreply@domain.com.
- heloDomain, the domain label presented by the client in HELO or EHLO.
- hostDomain, the domain of the current host performinng authentication.
- SpfExpression Array, (optional) SPF expressions to use if the domain doesn't provide records in DNS.

For example:

    var ipAddress = IPAddress.Parse("11.22.33.44");
    var result = SpfResolver.CheckHost(ipAddress, "zz.com", "noreply@zz.com", "zz.com", "aa.com");

`CheckHost` returns a `KeyValuePair<SpfResult, string>` where string is an explanation if any or simply the string representation of the `SpfResult`
