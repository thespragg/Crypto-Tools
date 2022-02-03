const onlyUnique = (value, index, self) => {
  return self.indexOf(value) === index;
};

const toCaps = (str) => {
  const words = str.split(" ");
  for (let i = 0; i < words.length; i++) {
    words[i] = words[i][0].toUpperCase() + words[i].substr(1);
  }
  return words.join(" ");
};

const dcaOptions = [
  {
    label: "Weekly",
    value: "weekly",
  },
  {
    label: "Monthly",
    value: "monthly",
  },
];

const chartOpts = {
  responsive: true,
  plugins: {
    legend: {
      display: false,
    },
    title: {
      display: true,
      text: "Portfolio value over time",
    },
  },
  elements: {
    line: {
      tension: 0.5,
    },
  },
  scales: {
    x: {
      grid: {
        display: false,
      },
    },
    y: {
      grid: {
        display: false,
      },
    },
  },
};

const getColor = (val, spent) => {
    if (val > spent) return "#31a843";
    if (val < spent) return "#c93a5c";
    return "#000";
  };
  
  const getProfitColor = (profit) => {
    if (profit > 0) return "#31a843";
    if (profit < 0) return "#c93a5c";
    return "#000";
  };

export default {
  onlyUnique,
  toCaps,
  dcaOptions,
  chartOpts,
  getColor,
  getProfitColor
};
