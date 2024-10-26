package main

import (
	"fmt"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {

		// try getting output from other project
		referencedStack, err := pulumi.NewStackReference(ctx, "organization/titeenipelifoundation/dev", nil)
		if err != nil {
			return fmt.Errorf("error referencing stack: %w", err)
		}
		outputValue := referencedStack.GetOutput(pulumi.String("clusterName"))
		outputValue2 := referencedStack.GetOutput(pulumi.String("domainName"))
		ctx.Export("referencedOutput", outputValue)
		ctx.Export("referencedOutput2", outputValue2)

		return nil
	})
}
