package main

import (
	"fmt"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {

		referencedStack, err := pulumi.NewStackReference(ctx, "organization/titeenipelifoundation/dev", nil)
		if err != nil {
			return fmt.Errorf("error referencing stack: %w", err)
		}
		kubeconfig := referencedStack.GetOutput(pulumi.String("kubeconfig")).AsStringOutput()
		domainName := referencedStack.GetOutput(pulumi.String("domainName")).AsStringOutput()
		titeenipeliRG := referencedStack.GetOutput(pulumi.String("titeenipeliRG")).AsStringOutput()
		certManagerIdentityClientId := referencedStack.GetOutput(pulumi.String("certManagerIdentityClientId")).AsStringOutput()
		externalDnsIdentityClientId := referencedStack.GetOutput(pulumi.String("externalDnsIdentityClientId")).AsStringOutput()
		traefikIdentityClientId := referencedStack.GetOutput(pulumi.String("traefikIdentityClientId")).AsStringOutput()
		publicIpName := referencedStack.GetOutput(pulumi.String("publicIpName")).AsStringOutput()

		k8sProvider, err := buildProvider(ctx, kubeconfig)
		if err != nil {
			return err
		}
		buildCharts(ctx, k8sProvider, domainName, certManagerIdentityClientId, externalDnsIdentityClientId, traefikIdentityClientId, titeenipeliRG, publicIpName)

		return nil
	})
}
