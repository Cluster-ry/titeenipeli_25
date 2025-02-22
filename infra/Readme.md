# Infra

This folder contains everything related to Azure IaaC infra configurations.

There the folder structure is as following:
- bicep
- pulumi
     - foundation
     - kube

With the Bicep containing files to initialize the azure account for Pulumi by creating necessary resources.
On the other hand pulumi folder contains two Pulumi projects inside it:
- Foundation
    - This is for the foundational resources like AKS and DNS
- kube
    - As the name suggests this is for kubernetes related resources that are deployed inside AKS