# Documentação Oficial e Desafio Técnico: Monitoramento CadastroAPI

Esta documentação atende aos requisitos obrigatórios para avaliação do projeto de infraestrutura, orquestração e monitoramento de aplicações. O projeto consiste em uma API desenvolvida em .NET conectada a um banco MySQL, rodando em um cluster Kubernetes com observabilidade completa via Prometheus e Grafana.

---

## 1. Validação dos Requisitos Obrigatórios

A arquitetura atual contempla 100% dos requisitos solicitados:

* **Cluster Local:** Implementado utilizando Kind/Minikube, totalmente funcional e gerenciado via `kubectl`.
* **Organização (Namespaces):** O cluster está isolado logicamente em dois namespaces principais:
    * `api`: Contém a aplicação .NET e o banco de dados MySQL.
    * `monitoring`: Contém a stack de observabilidade (Prometheus e Grafana).
* **Aplicação (Deployment, Réplicas e Service):** A API está rodando no cluster via um manifesto de `Deployment`. Para garantir alta disponibilidade (HA) e atender aos requisitos, foram configuradas **3 réplicas** simultâneas. A aplicação está exposta para consumo através de um `Service` (ClusterIP com Port-Forward).
* **Configuração de Recursos (CPU/Memória):** O manifesto da API possui limites estritos definidos para evitar esgotamento do nó:
    ```yaml
    resources:
      requests:
        memory: "256Mi"
        cpu: "200m"
      limits:
        memory: "512Mi"
        cpu: "500m"
    ```
* **Stack de Monitoramento e Dashboards:** Utilizamos Prometheus (coleta) e Grafana (visualização) integrados ao `cAdvisor` e `kube-state-metrics`. Existem dois dashboards funcionais principais em operação:
    * **Kubernetes Cluster Monitoring:** Exibe métricas de uso de CPU, Memória, I/O de rede e saúde dos pods/nós.
    * **.NET Prometheus:** Exibe a duração das requisições HTTP, contagem do Garbage Collector e threads da aplicação C#.

---

## 2. Desafio Técnico: Simulação e Resolução de Problema Real

Durante a implementação e os testes de carga da infraestrutura, enfrentamos um problema crítico de comunicação de rede interna que comprometeu a visibilidade do cluster. A análise aprofundada desse incidente compõe o nosso desafio técnico.

### O que aconteceu?
O dashboard principal de monitoramento do Kubernetes no Grafana passou a exibir telas vazias com o alerta "N/D" (Sem dados) em todos os gráficos de utilização de memória e CPU. O menu de seleção de servidores ("Nós") estava inacessível, indicando que o Grafana havia perdido a referência de quais máquinas existiam no cluster.

### Como identifiquei
O primeiro indício visual ocorreu diretamente nos painéis do Grafana. Para investigar a fundo, sabíamos que o Grafana é apenas o espelho visual; a fonte da verdade é o Prometheus. Acessamos a interface web do Prometheus e fomos até o menu **Status > Target health**.

### Onde olhei (logs, métricas, etc)?
Na interface de Targets do Prometheus, constatamos que o alvo responsável por ler o estado do cluster, chamado `kube-state-metrics`, estava marcado em vermelho com o status **DOWN**. O erro exato exibido nos logs do Prometheus (Error scraping target) era:

> `Get "http://kube-state-metrics.monitoring.svc.cluster.local:80/metrics": dial tcp: lookup kube-state-metrics.monitoring.svc.cluster.local on 10.96.0.10:53: no such host`

### Causa raiz?
A causa raiz foi um erro de **Resolução de DNS interno do Kubernetes atrelado a namespaces incorretos**.

O arquivo `configmap.yaml` do Prometheus foi instruído a buscar os dados no endereço `kube-state-metrics.monitoring.svc.cluster.local` (esperando que o serviço estivesse no namespace `monitoring`). No entanto, aplicamos o manifesto de instalação oficial do GitHub do `kube-state-metrics`, que por padrão implanta a ferramenta no namespace reservado `kube-system`. Como eles estavam em "bairros" (namespaces) diferentes, o Prometheus bateu em uma porta inexistente.

### Como resolvi
O processo de resolução seguiu três etapas para corrigir a rota e restabelecer o DNS:

1.  **Correção do Manifesto:** Abrimos o `configmap.yaml` e alteramos o FQDN (Fully Qualified Domain Name) do alvo no Prometheus, mudando de `monitoring` para `kube-system` e ajustando a porta para `8080` (padrão oficial). A linha ficou: `"kube-state-metrics.kube-system.svc.cluster.local:8080"`.
2.  **Aplicação da Configuração:** Rodamos o comando `kubectl apply -f configmap.yaml` para atualizar a configuração no banco do Kubernetes.
3.  **Recarregamento do Pod:** Para forçar o Prometheus a ler as novas rotas de DNS imediatamente, destruímos o pod antigo com `kubectl delete pods -l app=prometheus -n monitoring`. O Deployment automaticamente subiu um pod novo.

Após alguns segundos, o Target no Prometheus mudou para **UP** (verde) e os dashboards do Grafana voltaram a preencher os gráficos em tempo real.

---

## 3. Entregáveis

* **Repositório:** (https://github.com/Jonathan-EngSoftware/Monitoramento-Api-e-Cluster-Kubernetes.git) contendo todos os manifestos (YAML), Dockerfiles e o código C# da API.
* **Demonstrações:** O ambiente e as métricas geradas pela ferramenta de estresse (`hey`) estão documentados nas imagens e em execução no cluster local.

**Autor:** Jonathan Henrique Ribeiro Coutinho de Almeida
**Data:** Maio de 2026
