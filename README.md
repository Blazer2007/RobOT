# RobOT

Jogo 3D de puzzles e plataformas desenvolvido em Unity (C#) no âmbito da UC
de Técnicas Avançadas de Programação para Videojogos (2026/27, Escola Superior
de Tecnologia de Setúbal).

## Sobre o jogo

Rob desce a um complexo subterrâneo abandonado para reabrir a câmara das
sementes antes que o último sistema de energia falhe. Pelo caminho encontra
O.T, o velho robô de manutenção do seu mentor, desligado e quase sem
componentes.

O jogador controla as duas personagens e alterna entre elas. Cada sala é uma
câmara de teste com puzzles e obstáculos que desafiam os dois ao mesmo tempo.
Para chegar à sala seguinte, é preciso usar os componentes do O.T e encontrar
novos módulos escondidos pelo caminho, que lhe dão novas capacidades.

## Funcionalidades

- Salas-puzzle com dificuldade crescente
- Duas personagens controláveis (Rob e O.T) com troca de controlo
- Componentes modulares do robô que desbloqueiam novas capacidades
- Obstáculos e puzzles que exigem coordenação entre as duas personagens
- Menu principal e menu de pausa, com ecrã de settings
- Sistema de cheats para demonstrar todas as funcionalidades

## Como jogar

| Ação | Controlo |
| --- | --- |
| Mover | WASD |
| Câmara | Rato |
| Saltar | Espaço |
| Interagir | E |
| Alternar entre Rob e O.T | Tab |
| Usar componente | Clique esquerdo / Q |
| Menu de pausa | Esc |

Os controlos podem mudar ao longo do desenvolvimento.

## Arquitetura

O projeto privilegia uma arquitetura extensível, com:

- interfaces para interações, capacidades do robô e condições de desbloqueio
- composição de comportamentos reutilizáveis
- comunicação por eventos entre sistemas
- dados de configuração em ScriptableObjects, separados da lógica

## Tecnologias

- Unity 6000.3.25f1 · C#
- Blender (modelação 3D)

## Estrutura do repositório

- `src/` – projeto Unity
- `build/` – build do jogo

## Equipa

- Jorge Silva (líder, modelação 3D)
- Pedro Moretti (programação)
- Alexandre Andrade (programação)

## Estado do projeto

Em desenvolvimento. Entrega final a 18 de janeiro de 2027.
