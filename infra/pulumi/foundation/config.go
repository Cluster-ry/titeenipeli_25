package main

import (
	"github.com/pulumi/pulumi-random/sdk/v4/go/random"
	"github.com/pulumi/pulumi-tls/sdk/v5/go/tls"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi/config"
)

type Config struct {
	K8sVersion       string
	Password         pulumi.StringInput
	GeneratedKeyPair *tls.PrivateKey
	AdminUserName    string
	SshPublicKey     pulumi.StringInput
	NodeCount        int
	MaxNodeCount     int
	NodeSize         string
	BackNodeSize     string
	BaseDomain       string
	BaseConfigDomain string
	ClusterName      string
}

func configure(ctx *pulumi.Context) (Config, error) {
	out := Config{}

	cfg := config.New(ctx, "")

	out.K8sVersion = cfg.Get("k8sVersion")
	if out.K8sVersion == "" {
		out.K8sVersion = "1.31.2"
	}

	generatedKeyPair, err := tls.NewPrivateKey(ctx, "ssh-key",
		&tls.PrivateKeyArgs{
			Algorithm: pulumi.String("RSA"),
			RsaBits:   pulumi.Int(4096),
		})
	if err != nil {
		return Config{}, err
	}
	out.GeneratedKeyPair = generatedKeyPair

	pw := cfg.Get("password")
	if pw == "" {
		randPW, err := random.NewRandomPassword(ctx, "pw",
			&random.RandomPasswordArgs{
				Length:  pulumi.Int(20),
				Special: pulumi.Bool(true),
			})

		if err != nil {
			return Config{}, err
		}

		out.Password = randPW.Result
	} else {
		out.Password = pulumi.String(pw)
	}

	out.AdminUserName = cfg.Get("adminUserName")
	if out.AdminUserName == "" {
		out.AdminUserName = "testuser"
	}

	sshPubKey := cfg.Get("sshPublicKey")
	if sshPubKey == "" {
		out.SshPublicKey = generatedKeyPair.PublicKeyOpenssh
	} else {
		out.SshPublicKey = pulumi.String(sshPubKey)
	}

	out.NodeCount = cfg.GetInt("nodeCount")
	if out.NodeCount == 0 {
		out.NodeCount = 1
	}

	out.MaxNodeCount = cfg.GetInt("MaxNodeCount")
	if out.MaxNodeCount == 0 {
		out.MaxNodeCount = 5
	}

	out.NodeSize = cfg.Get("nodeSize")
	if out.NodeSize == "" {
		out.NodeSize = "Standard_D4ads_v5"
	}

	out.BackNodeSize = cfg.Get("BackNodeSize")
	if out.BackNodeSize == "" {
		out.BackNodeSize = "Standard_F4s_v2"
	}

	out.BaseDomain = cfg.Get("domain")
	if out.BaseDomain == "" {
		out.BaseDomain = "titeen.it"
	}

	out.BaseConfigDomain = cfg.Get("configDomain")
	if out.BaseConfigDomain == "" {
		out.BaseConfigDomain = "cluster2017.fi"
	}

	out.ClusterName = cfg.Get("clusterName")
	if out.ClusterName == "" {
		out.ClusterName = "titeenipeli-k8s"
	}

	return out, nil
}
