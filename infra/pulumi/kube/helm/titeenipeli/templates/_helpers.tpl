{{/*
Name
*/}}
{{- define "titeenipeli.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end }}

{{/*
Common labels
*/}}
{{- define "titeenipeli.labels" -}}
name: {{ include "titeenipeli.name" . }}
component: microservice
project: {{ include "titeenipeli.name" . }}
{{ include "titeenipeli.selectorLabels" . }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "titeenipeli.selectorLabels" -}}
app.kubernetes.io/instance: {{ include "titeenipeli.name" . }}
app.kubernetes.io/name: {{ .Release.Name }}
{{- end }}

{{/*
Name for frontend
*/}}
{{- define "titeenipeli.frontendName" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 57 | trimSuffix "-" -}}-front
{{- end }}

{{/*
Common labels for frontend
*/}}
{{- define "titeenipeli.frontendLabels" -}}
name: {{ include "titeenipeli.frontendName" . }}
component: microservice
project: {{ include "titeenipeli.frontendName" . }}
{{ include "titeenipeli.frontendSelectorLabels" . }}
{{- end }}

{{/*
Selector labels for frontend
*/}}
{{- define "titeenipeli.frontendSelectorLabels" -}}
app.kubernetes.io/instance: {{ include "titeenipeli.frontendName" . }}
app.kubernetes.io/name: {{ .Release.Name }}-front
{{- end }}

{{/*
Name for backend
*/}}
{{- define "titeenipeli.backendName" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 58 | trimSuffix "-" -}}-back
{{- end }}

{{/*
Common labels for backend
*/}}
{{- define "titeenipeli.backendLabels" -}}
name: {{ include "titeenipeli.backendName" . }}
component: microservice
project: {{ include "titeenipeli.backendName" . }}
{{ include "titeenipeli.backendSelectorLabels" . }}
{{- end }}

{{/*
Selector labels for backend
*/}}
{{- define "titeenipeli.backendSelectorLabels" -}}
app.kubernetes.io/instance: {{ include "titeenipeli.backendName" . }}
app.kubernetes.io/name: {{ .Release.Name }}-back
{{- end }}

{{/*
Name for bot
*/}}
{{- define "titeenipeli.botName" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 59 | trimSuffix "-" -}}-bot
{{- end }}

{{/*
Common labels for bot
*/}}
{{- define "titeenipeli.botLabels" -}}
name: {{ include "titeenipeli.botName" . }}
component: microservice
project: {{ include "titeenipeli.botName" . }}
{{ include "titeenipeli.botSelectorLabels" . }}
{{- end }}

{{/*
Selector labels for bot
*/}}
{{- define "titeenipeli.botSelectorLabels" -}}
app.kubernetes.io/instance: {{ include "titeenipeli.botName" . }}
app.kubernetes.io/name: {{ .Release.Name }}-bot
{{- end }}