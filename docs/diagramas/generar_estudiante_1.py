"""Regenera los UML del informe: Python 3 + Pillow, sin dependencias del juego.

Las relaciones muestran las clases relevantes de cada patrón; se omiten firmas
completas y miembros auxiliares. Las fuentes Mermaid completas están en PATRONES.md.
"""

from html import escape
from math import hypot
from pathlib import Path
import os
import re

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
INK = "#183348"
ACCENT = "#166579"
FONT_DIR = Path(os.environ.get("WINDIR", "C:/Windows")) / "Fonts"


class Diagram:
    def __init__(self, title, height=1000):
        self.width, self.height = 1600, height
        self.image = Image.new("RGB", (self.width, height), "white")
        self.draw = ImageDraw.Draw(self.image)
        self.svg = [f'<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="{height}" viewBox="0 0 1600 {height}">', '<rect width="100%" height="100%" fill="white"/>']
        self.text((40, 25), title, 42, bold=True, color=ACCENT)

    def text(self, xy, value, size=30, bold=False, color=INK):
        filename = "arialbd.ttf" if bold else "arial.ttf"
        path = FONT_DIR / filename
        font = ImageFont.truetype(str(path) if path.exists() else "DejaVuSans.ttf", size)
        self.draw.text(xy, value, fill=color, font=font)
        weight = "bold" if bold else "normal"
        self.svg.append(f'<text x="{xy[0]}" y="{xy[1] + size}" font-family="Arial, sans-serif" font-size="{size}" font-weight="{weight}" fill="{color}">{escape(value)}</text>')

    def line(self, points, dashed=False, color=INK, width=3):
        for a, b in zip(points, points[1:]):
            distance = hypot(b[0] - a[0], b[1] - a[1])
            if dashed and distance:
                for i in range(0, int(distance), 18):
                    lo, hi = i / distance, min(i + 10, distance) / distance
                    self.draw.line([(a[0] + (b[0] - a[0]) * lo, a[1] + (b[1] - a[1]) * lo), (a[0] + (b[0] - a[0]) * hi, a[1] + (b[1] - a[1]) * hi)], fill=color, width=width)
            else:
                self.draw.line([a, b], fill=color, width=width)
        dash = ' stroke-dasharray="10 8"' if dashed else ""
        coords = " ".join(f"{x},{y}" for x, y in points)
        self.svg.append(f'<polyline points="{coords}" fill="none" stroke="{color}" stroke-width="{width}"{dash}/>')

    def polygon(self, points, fill="white"):
        self.draw.polygon(points, fill=fill, outline=INK, width=3)
        coords = " ".join(f"{x},{y}" for x, y in points)
        self.svg.append(f'<polygon points="{coords}" fill="{fill}" stroke="{INK}" stroke-width="3"/>')

    def edge(self, points, triangle=False, dashed=False):
        self.line(points, dashed)
        a, b = points[-2:]
        length = hypot(b[0] - a[0], b[1] - a[1])
        ux, uy = (b[0] - a[0]) / length, (b[1] - a[1]) / length
        size = 23 if triangle else 15
        left = (b[0] - ux * size - uy * size / 2, b[1] - uy * size + ux * size / 2)
        right = (b[0] - ux * size + uy * size / 2, b[1] - uy * size - ux * size / 2)
        if triangle:
            self.polygon([b, left, right])
        else:
            self.line([left, b, right])

    def box(self, rect, title, lines=(), kind=None):
        x, y, w, h = rect
        header = 105 if kind else 65
        self.draw.rectangle((x, y, x + w, y + h), fill="#F6FAFC", outline=INK, width=3)
        self.svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="#F6FAFC" stroke="{INK}" stroke-width="3"/>')
        if kind:
            self.text((x + 17, y + 10), f"«{kind}»", 28, color=ACCENT)
        self.text((x + 17, y + (47 if kind else 15)), title, 32, bold=True)
        if lines:
            self.line([(x, y + header), (x + w, y + header)])
            for i, value in enumerate(lines):
                self.text((x + 17, y + header + 14 + i * 42), value, 30)

    def save(self, stem):
        self.text((40, self.height - 49), "UML parcial · + público · # protegido · triángulo: herencia/realización · flecha: uso/referencia", 25, color="#526875")
        self.svg.append("</svg>")
        self.image.save(ROOT / f"{stem}.png")
        (ROOT / f"{stem}.svg").write_text("\n".join(self.svg), encoding="utf-8")


def builder():
    d = Diagram("Builder · construcción por pasos", 900)
    d.box((530, 110, 540, 390), "IBuilderUnidad", ["+ DefinirIdentidad(...)", "+ SeleccionarRol(...)", "+ ConfigurarEstadisticas(...)", "+ ConfigurarEquipamiento(...)", "+ Ubicar(...)", "+ Construir(): Unidad"], "interface")
    d.box((35, 175, 430, 200), "DirectorEquipo", ["+ CrearExploracion(origen)", "+ CrearAsalto(origen)"])
    d.box((1135, 175, 430, 155), "ServicioPreparacion", ["+ CrearUnidad(...)"])
    d.box((530, 650, 540, 145), "UnidadBuilder", ["+ Construir(): Unidad"])
    d.box((1135, 650, 430, 145), "Unidad", ["Producto completo"])
    d.edge([(800, 650), (800, 500)], triangle=True, dashed=True)
    d.edge([(250, 375), (250, 565), (590, 565), (590, 650)])
    d.text((270, 520), "dirige recetas", 29)
    d.edge([(1350, 330), (1350, 565), (1000, 565), (1000, 650)])
    d.text((1145, 520), "configura pasos", 29)
    d.edge([(1070, 720), (1135, 720)])
    d.text((1080, 675), "crea", 26)
    d.save("builder")


def prototype():
    d = Diagram("Prototype · variantes independientes", 1000)
    d.box((35, 130, 490, 285), "RegistroPrototipos", ["+ Registrar(id, plantilla)", "+ Consultar(id): VistaUnidad", "+ Clonar(id): Unidad", "+ CrearVariante(...): Unidad"])
    d.box((600, 110, 440, 160), "IPrototipoUnidad", ["+ Clonar(): Unidad"], "interface")
    d.box((600, 440, 440, 205), "Unidad", ["+ Clonar(): Unidad", "+ ClonarConAjustes(...)", "Nuevo Guid por clon"])
    d.box((1120, 130, 445, 155), "IEspecie", ["Referencia inmutable"], "interface")
    d.box((35, 750, 490, 155), "ServicioPreparacion", ["Solicita clones y variantes"])
    d.box((1120, 440, 445, 150), "Estadisticas", ["+ Clonar()"])
    d.box((600, 750, 440, 175), "IItem", ["+ Copiar()"], "interface")
    d.box((1120, 750, 445, 175), "IEstadoPersonaje", ["+ Clonar()"], "interface")
    d.edge([(820, 440), (820, 270)], triangle=True, dashed=True)
    d.edge([(280, 750), (280, 415)])
    d.text((65, 650), "usa el registro", 28)
    d.edge([(480, 415), (480, 525), (600, 525)])
    d.text((45, 465), "protege originales", 28)
    d.text((45, 505), "y devuelve clones", 28)
    d.edge([(1040, 475), (1080, 475), (1080, 210), (1120, 210)])
    d.text((1150, 335), "comparte especie", 28)
    d.edge([(1040, 540), (1120, 540)])
    d.edge([(820, 645), (820, 750)])
    d.text((850, 690), "copia inventario", 27)
    d.edge([(1040, 600), (1080, 600), (1080, 825), (1120, 825)])
    d.text((1140, 650), "copia estado", 28)
    d.save("prototype")


def flyweight():
    d = Diagram("Flyweight · 10.000 tripulantes / 5 especies", 1100)
    d.box((35, 125, 580, 330), "FabricaEspecies", ["- Dictionary<string, IEspecie>", "+ ObtenerEspecie(nombre)", "+ ObtenerTodas()", "+ CantidadCreadas"], None)
    d.box((1060, 125, 505, 335), "IEspecie", ["Nombre · Color", "PlanetaOrigen · Dieta", "Estado intrínseco inmutable"], "interface")
    d.box((1060, 615, 505, 145), "Especie", ["Implementa datos de especie"])
    d.box((655, 615, 355, 300), "Tripulante", ["Id · Nombre · Rango", "Especie: IEspecie", "Datos particulares"])
    d.box((35, 615, 565, 180), "DemostradorFlyweight", ["+ Generar()", "Cuenta referencias reales"])
    d.box((35, 900, 565, 135), "ServicioPreparacion", ["Expone la demostración"])
    d.edge([(615, 250), (1060, 250)])
    d.text((685, 205), "caché por nombre", 29)
    d.edge([(1310, 615), (1310, 460)], triangle=True, dashed=True)
    d.edge([(830, 615), (830, 395), (1060, 395)])
    d.text((680, 480), "comparte", 29)
    d.edge([(315, 615), (315, 455)])
    d.text((75, 510), "solicita 5 instancias", 29)
    d.edge([(600, 705), (655, 705)])
    d.text((410, 825), "genera 10.000", 27)
    d.edge([(315, 900), (315, 795)])
    d.save("flyweight")


def template_method():
    d = Diagram("Template Method · algoritmo fijo / cuatro roles", 1160)
    d.box((500, 110, 610, 345), "TurnoUnidad", ["+ EjecutarTurno() [no virtual]", "# Iniciar() [virtual]", "# Decidir() [abstracto]", "# Actuar() [abstracto]", "# Finalizar() [virtual]"], "abstract")
    d.box((35, 145, 410, 235), "ServicioTurnos", ["+ EjecutarTurno(id)", "Crea según el rol", "Conserva el historial"])
    d.edge([(445, 270), (500, 270)], dashed=True)
    names = ["TurnoSoldado", "TurnoMedico", "TurnoFrancotirador", "TurnoComandante"]
    for i, name in enumerate(names):
        x = 20 + i * 400
        d.box((x, 600, 360, 190), name, ["# Decidir()", "# Actuar()"])
        d.line([(x + 180, 600), (x + 180, 520), (805, 520)])
        d.line([(x + 180, 790), (x + 180, 835), (805, 835)], dashed=True)
    d.edge([(805, 520), (805, 455)], triangle=True)
    d.edge([(805, 835), (805, 900)], dashed=True)
    d.text((845, 840), "usan el mismo contexto de misión", 28)
    d.box((495, 900, 620, 180), "ContextoTurno", ["+ Mover(...) · Atacar(...) · Curar(...)", "Referencia a IServicioCombate"])
    d.save("template-method")


if __name__ == "__main__":
    for render in (builder, prototype, flyweight, template_method):
        render()
    source = (ROOT.parent / "PATRONES.md").read_text(encoding="utf-8")
    for heading, block in re.findall(r"## (Builder|Prototype|Flyweight|Template Method)\n.*?```mermaid\n(.*?)```", source, re.S):
        stem = heading.lower().replace(" ", "-")
        (ROOT / f"{stem}.mmd").write_text(block, encoding="utf-8")
    print("Cuatro diagramas generados en PNG, SVG y Mermaid.")
