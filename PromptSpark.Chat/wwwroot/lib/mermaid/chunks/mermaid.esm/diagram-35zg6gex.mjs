import {
  populateCommonDb
} from "./chunk-K2ZEYYM2.mjs";
import {
  parse
} from "./chunk-ZTYE4VVB.mjs";
import "./chunk-VUYTGWVG.mjs";
import "./chunk-FKTRZSYK.mjs";
import "./chunk-55U4IUQG.mjs";
import {
  selectSvgElement
} from "./chunk-XB7NGBBB.mjs";
import {
  cleanAndMerge
} from "./chunk-TVJ2DRIE.mjs";
import "./chunk-GKOISANM.mjs";
import {
  clear,
  configureSvgSize,
  defaultConfig_default,
  getAccDescription,
  getAccTitle,
  getConfig,
  getDiagramTitle,
  log,
  setAccDescription,
  setAccTitle,
  setDiagramTitle
} from "./chunk-M7TDSP4I.mjs";
import "./chunk-5PULXXUT.mjs";
import "./chunk-ACX3ZYE2.mjs";
import "./chunk-46UCTVYF.mjs";
import "./chunk-TGZYFRKZ.mjs";
import "./chunk-GRZAG2UZ.mjs";
import "./chunk-HD3LK5B5.mjs";
import {
  __name
} from "./chunk-DLQEHMXD.mjs";

// src/diagrams/packet/db.ts
var defaultPacketData = {
  packet: []
};
var data = structuredClone(defaultPacketData);
var DEFAULT_PACKET_CONFIG = defaultConfig_default.packet;
var getConfig2 = /* @__PURE__ */ __name(() => {
  const config = cleanAndMerge({
    ...DEFAULT_PACKET_CONFIG,
    ...getConfig().packet
  });
  if (config.showBits) {
    config.paddingY += 10;
  }
  return config;
}, "getConfig");
var getPacket = /* @__PURE__ */ __name(() => data.packet, "getPacket");
var pushWord = /* @__PURE__ */ __name((word) => {
  if (word.length > 0) {
    data.packet.push(word);
  }
}, "pushWord");
var clear2 = /* @__PURE__ */ __name(() => {
  clear();
  data = structuredClone(defaultPacketData);
}, "clear");
var db = {
  pushWord,
  getPacket,
  getConfig: getConfig2,
  clear: clear2,
  setAccTitle,
  getAccTitle,
  setDiagramTitle,
  getDiagramTitle,
  getAccDescription,
  setAccDescription
};

// src/diagrams/packet/parser.ts
var maxPacketSize = 1e4;
var populate = /* @__PURE__ */ __name((ast) => {
  populateCommonDb(ast, db);
  let lastByte = -1;
  let word = [];
  let row = 1;
  const { bitsPerRow } = db.getConfig();
  for (let { start, end, label } of ast.blocks) {
    if (end && end < start) {
      throw new Error(`Packet block ${start} - ${end} is invalid. End must be greater than start.`);
    }
    if (start !== lastByte + 1) {
      throw new Error(
        `Packet block ${start} - ${end ?? start} is not contiguous. It should start from ${lastByte + 1}.`
      );
    }
    lastByte = end ?? start;
    log.debug(`Packet block ${start} - ${lastByte} with label ${label}`);
    while (word.length <= bitsPerRow + 1 && db.getPacket().length < maxPacketSize) {
      const [block, nextBlock] = getNextFittingBlock({ start, end, label }, row, bitsPerRow);
      word.push(block);
      if (block.end + 1 === row * bitsPerRow) {
        db.pushWord(word);
        word = [];
        row++;
      }
      if (!nextBlock) {
        break;
      }
      ({ start, end, label } = nextBlock);
    }
  }
  db.pushWord(word);
}, "populate");
var getNextFittingBlock = /* @__PURE__ */ __name((block, row, bitsPerRow) => {
  if (block.end === void 0) {
    block.end = block.start;
  }
  if (block.start > block.end) {
    throw new Error(`Block start ${block.start} is greater than block end ${block.end}.`);
  }
  if (block.end + 1 <= row * bitsPerRow) {
    return [block, void 0];
  }
  return [
    {
      start: block.start,
      end: row * bitsPerRow - 1,
      label: block.label
    },
    {
      start: row * bitsPerRow,
      end: block.end,
      label: block.label
    }
  ];
}, "getNextFittingBlock");
var parser = {
  parse: /* @__PURE__ */ __name(async (input) => {
    const ast = await parse("packet", input);
    log.debug(ast);
    populate(ast);
  }, "parse")
};

// src/diagrams/packet/renderer.ts
var draw = /* @__PURE__ */ __name((_text, id, _version, diagram2) => {
  const db2 = diagram2.db;
  const config = db2.getConfig();
  const { rowHeight, paddingY, bitWidth, bitsPerRow } = config;
  const words = db2.getPacket();
  const title = db2.getDiagramTitle();
  const totalRowHeight = rowHeight + paddingY;
  const svgHeight = totalRowHeight * (words.length + 1) - (title ? 0 : rowHeight);
  const svgWidth = bitWidth * bitsPerRow + 2;
  const svg = selectSvgElement(id);
  svg.attr("viewbox", `0 0 ${svgWidth} ${svgHeight}`);
  configureSvgSize(svg, svgHeight, svgWidth, config.useMaxWidth);
  for (const [word, packet] of words.entries()) {
    drawWord(svg, packet, word, config);
  }
  svg.append("text").text(title).attr("x", svgWidth / 2).attr("y", svgHeight - totalRowHeight / 2).attr("dominant-baseline", "middle").attr("text-anchor", "middle").attr("class", "packetTitle");
}, "draw");
var drawWord = /* @__PURE__ */ __name((svg, word, rowNumber, { rowHeight, paddingX, paddingY, bitWidth, bitsPerRow, showBits }) => {
  const group = svg.append("g");
  const wordY = rowNumber * (rowHeight + paddingY) + paddingY;
  for (const block of word) {
    const blockX = block.start % bitsPerRow * bitWidth + 1;
    const width = (block.end - block.start + 1) * bitWidth - paddingX;
    group.append("rect").attr("x", blockX).attr("y", wordY).attr("width", width).attr("height", rowHeight).attr("class", "packetBlock");
    group.append("text").attr("x", blockX + width / 2).attr("y", wordY + rowHeight / 2).attr("class", "packetLabel").attr("dominant-baseline", "middle").attr("text-anchor", "middle").text(block.label);
    if (!showBits) {
      continue;
    }
    const isSingleBlock = block.end === block.start;
    const bitNumberY = wordY - 2;
    group.append("text").attr("x", blockX + (isSingleBlock ? width / 2 : 0)).attr("y", bitNumberY).attr("class", "packetByte start").attr("dominant-baseline", "auto").attr("text-anchor", isSingleBlock ? "middle" : "start").text(block.start);
    if (!isSingleBlock) {
      group.append("text").attr("x", blockX + width).attr("y", bitNumberY).attr("class", "packetByte end").attr("dominant-baseline", "auto").attr("text-anchor", "end").text(block.end);
    }
  }
}, "drawWord");
var renderer = { draw };

// src/diagrams/packet/styles.ts
var defaultPacketStyleOptions = {
  byteFontSize: "10px",
  startByteColor: "black",
  endByteColor: "black",
  labelColor: "black",
  labelFontSize: "12px",
  titleColor: "black",
  titleFontSize: "14px",
  blockStrokeColor: "black",
  blockStrokeWidth: "1",
  blockFillColor: "#efefef"
};
var styles = /* @__PURE__ */ __name(({ packet } = {}) => {
  const options = cleanAndMerge(defaultPacketStyleOptions, packet);
  return `
	.packetByte {
		font-size: ${options.byteFontSize};
	}
	.packetByte.start {
		fill: ${options.startByteColor};
	}
	.packetByte.end {
		fill: ${options.endByteColor};
	}
	.packetLabel {
		fill: ${options.labelColor};
		font-size: ${options.labelFontSize};
	}
	.packetTitle {
		fill: ${options.titleColor};
		font-size: ${options.titleFontSize};
	}
	.packetBlock {
		stroke: ${options.blockStrokeColor};
		stroke-width: ${options.blockStrokeWidth};
		fill: ${options.blockFillColor};
	}
	`;
}, "styles");

// src/diagrams/packet/diagram.ts
var diagram = {
  parser,
  db,
  renderer,
  styles
};
export {
  diagram
};
