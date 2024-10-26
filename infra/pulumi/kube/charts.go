package main

import (
	"fmt"
	"os"

	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi/config"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
	"gopkg.in/yaml.v2"
)

func buildCharts(
	ctx *pulumi.Context,
	k8sProvider *kubernetes.Provider,
	domainName pulumi.StringOutput,
	certManagerIdentityClientId pulumi.StringOutput, titeenipeliRG pulumi.StringOutput) error {

	subscription, err := core.LookupSubscription(ctx, nil)
	if err != nil {
		return err
	}

	conf := config.New(ctx, "")
	email := conf.RequireSecret("email")

	v1.NewNamespace(ctx, "cert-manager", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("cert-manager"),
		}},
		pulumi.Providers(k8sProvider))

	values, err := mapValues("./values/cert-manager/values.yaml")
	if err != nil {
		return err
	}

	certManagerChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("cert-manager"),
		Version: pulumi.String("v1.15.3"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://charts.jetstack.io"),
		},
		Namespace: pulumi.String("cert-manager"),
		Values:    pulumi.Map(values),
	}

	helm.NewChart(ctx, "cert-manager", certManagerChartArgs,
		pulumi.Providers(k8sProvider))

	helm.NewChart(ctx, "certmanager-certs", helm.ChartArgs{
		Path: pulumi.String("./helm/certmanager-certs"),
		Values: pulumi.Map{
			"hostedZoneName": domainName,
			"clientID":       certManagerIdentityClientId,
			"subscriptionID": pulumi.String(subscription.SubscriptionId),
			"titeenipeliRG":  titeenipeliRG,
			"email":          email,
		},
	}, pulumi.Provider(k8sProvider))

	return nil
}

func mapValues(path string) (pulumi.Map, error) {
	valuesData, err := os.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("unable to read values.yaml: %w", err)
	}

	var values map[interface{}]interface{}
	if err := yaml.Unmarshal(valuesData, &values); err != nil {
		return nil, fmt.Errorf("unable to parse values.yaml: %w", err)
	}

	pulumiMap := make(pulumi.Map)
	for k, v := range convertMap(values) {
		pulumiMap[k] = pulumi.Any(v)
	}

	return pulumiMap, nil
}

func convertMap(in map[interface{}]interface{}) map[string]interface{} {
	out := make(map[string]interface{})
	for k, v := range in {
		key, ok := k.(string)
		if !ok {
			continue
		}
		out[key] = convertValue(v)
	}
	return out
}

func convertValue(v interface{}) interface{} {
	switch val := v.(type) {
	case map[interface{}]interface{}:
		return convertMap(val)
	case []interface{}:
		for i, elem := range val {
			val[i] = convertValue(elem)
		}
	}
	return v
}
