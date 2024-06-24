# Backend

Primary source code of the backend.

## Table of Contents

<!-- TOC -->
* [Backend](#backend)
  * [Table of Contents](#table-of-contents)
  * [Required secrets](#required-secrets)
  * [Setup for development](#setup-for-development)
    * [Prerequisites](#prerequisites)
    * [Setup](#setup)
<!-- TOC -->

## Required secrets

Running backend requires some secrets to be set using `dotnet user-secrets`.
Syntax for setting secrets is `dotnet user-secrets set "<key>" "<value>" --project "<project name>"`.
Project name should be `"Titeenipeli"`. More [info](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

Secrets:
```json lines
"JWT:Secret"      // 256 characters long
"JWT:Encryption"  //  32 characters long
"ConnectionStrings:Database"  // Should contain Server, Port, Userid, Database
```

## Setup for development

### Prerequisites

- PostgreSQL database

### Setup

1. Add user to PostgreSQL with `Create DB` attribute
2. Fill required secrets (see [Required secrets](#required-secrets))
3. Run `Titeenipeli: http`