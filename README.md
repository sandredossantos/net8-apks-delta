# net8-apks-delta

A ideia desse projeto é replicar a solucao utilizada no [archive-patcher](https://github.com/google/archive-patcher). O foco inicial é a utilizacao do método GenerateDelta que fica na classe FileByFileV1DeltaGenerator, partindo dele resolver todas as dependencias.

# Problemáticas

- Tipos enumerados que em Java podem ter contrutor e comportamento
- Classes genéricas recebendo Void como parametro
