<template>
  <div class="flex justify-center flex-col p-6">
    <div
      class="
        w-full
        rounded
        bg-gray-200
        flex
        justify-center
        flex-col
        items-center
      "
    >
      <p class="p-6 text-lg">
        This strategy simulates what dollar cost averaging into the top coins by
        market cap over time would be.
      </p>
      <div class="h-px w-10/12 bg-gray-800" />
      <p class="text-red-700 font-bold mt-6" v-if="err">{{ err }}</p>
      <div class="flex flex-col sm:hidden p-6 w-full text-left justify-center">
        <div class="w-full flex items-center justify-center">
          <p class="w-1/2">DCA Amount</p>
          <n-input-number
            v-model:value="amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">DCA Interval</p>
          <n-select v-model:value="interval" :options="options" class="w-1/2" />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">Start Date</p>
          <n-date-picker
            v-model:value="start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">End Date</p>
          <n-date-picker
            v-model:value="end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
      </div>
      <div
        class="
          p-6
          font-bold
          flex-col
          w-full
          items-center
          justify-center
          hidden
          sm:flex
        "
      >
        <div class="flex w-full mb-2 items-center justify-center">
          <p class="w-1/5">DCA Amount</p>
          <p class="w-1/5">DCA Interval</p>
          <p class="w-1/5">Start Date</p>
          <p class="w-1/5">End Date</p>
        </div>
        <n-input-group class="justify-center">
          <n-input-number
            v-model:value="amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/5"
          />
          <n-select v-model:value="interval" :options="options" class="w-1/5" />
          <n-date-picker
            v-model:value="start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
          <n-date-picker
            v-model:value="end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
        </n-input-group>
        <div class="w-full flex justify-center flex-col items-center">
          <div class="my-2 flex text-lg">
            <p class="mr-4">Portfolio Allocation</p>
            <p :style="{ color: allocationColor }">{{ totalPercent }}%</p>
          </div>
          <div
            v-for="coin in portfolio"
            :key="coin.name"
            class="
              flex
              border border-solid border-slate-300
              items-center
              bg-white
              w-96
            "
          >
            <n-input-group class="justify-center w-full">
              <n-select
                v-model:value="coin.name"
                :options="coinOpts"
                placeholder="Coin"
                class="w-3/5"
              >
                <template #action>
                  <n-input v-model:value="search" placeholder="search" />
                </template>
              </n-select>
              <n-input-number
                v-model:value="coin.allocation"
                placeholder="Amount"
                :min="0"
                :max="100"
                class="w-2/5"
            /></n-input-group>
          </div>
          <p @click="addNew" class="cursor-pointer mt-2">+Add coin</p>
        </div>
        <n-button type="info" class="bg-blue-600 ml-2 mt-4 w-48" @click="run">
          Run
        </n-button>
      </div>
    </div>
    <div
      class="w-full rounded bg-gray-200 mt-6"
      v-if="portfolioProfits || running"
    >
      <n-spin :size="100" class="p-12" v-if="running">
        <template #description> Running Backtest </template>
      </n-spin>
      <div
        v-if="portfolioProfits"
        class="flex justify-center flex-col items-center"
      >
        <LineChart
          :chartData="chartData"
          class="w-11/12 h-96"
          :options="chartOpts"
        />
        <n-table :bordered="false" :single-line="false" class="w-5/6 my-6">
          <thead>
            <tr>
              <th>Date</th>
              <th>Portfolio Value</th>
              <th>Spent</th>
              <th>Profit/Loss</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="month in portfolioProfits" :key="month.date">
              <td>{{ new Date(month.date).toLocaleDateString("en-GB") }}</td>
              <td>
                {{
                  month.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                }}
              </td>
              <td>
                {{
                  month.spent.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                }}
              </td>
              <td
                :style="{ color: getProfitColor(month.profit) }"
                class="font-bold"
              >
                ${{
                  month.profit.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                }}
              </td>
            </tr>
          </tbody>
        </n-table>
        <n-table :bordered="false" :single-line="false" class="w-5/6 my-6">
          <thead>
            <tr>
              <th>Coin</th>
              <th>Profit/Loss</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="coin in coinProfits" :key="coin.name">
              <td>{{ coin.name }}</td>
              <td
                :style="{ color: getProfitColor(coin.profit) }"
                class="font-bold"
              >
                ${{
                  coin.profit
                    .toFixed(2)
                    .toString()
                    .replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                }}
              </td>
            </tr>
          </tbody>
        </n-table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { inject, ref, computed } from "vue";
import {
  NInput,
  NInputNumber,
  NSelect,
  NDatePicker,
  NButton,
  NTable,
  NInputGroup,
  NSpin,
} from "naive-ui";

import { Chart, registerables } from "chart.js";
Chart.register(...registerables);
import { LineChart } from "vue-chart-3";
const http = inject("$http");

const portfolio = ref([]);
const amount = ref(100);
const interval = ref("monthly");
const start = ref(Date.now() - 86400000 * 7);
const end = ref(Date.now());
const err = ref(null);
const dateDisabled = (ts) => ts < new Date("2013-01-01");
const search = ref("");

const props = defineProps({
  coins: {
    required: true,
    type: Array,
  },
});

const totalPercent = computed(() =>
  portfolio.value.reduce((a, b) => {
    return a + b["allocation"];
  }, 0)
);

const allocationColor = computed(() =>
  totalPercent.value <= 100 ? "#31a843" : "#c93a5c"
);

const coinOpts = computed(() =>
  props.coins
    .filter((x) => !search || x.toLowerCase().includes(search.value.toLowerCase()))
    .sort((a, b) => a.localeCompare(b))
    .map((x) => ({
      label: toCaps(x.replace(/-/g, " ")),
      value: x,
    }))
);

const toCaps = (str) => {
  const words = str.split(" ");

  for (let i = 0; i < words.length; i++) {
    words[i] = words[i][0].toUpperCase() + words[i].substr(1);
  }

  return words.join(" ");
};

const options = [
  {
    label: "Weekly",
    value: "weekly",
  },
  {
    label: "Monthly",
    value: "monthly",
  },
];

const portfolioProfits = ref(null);
const chartData = ref(null);
const coinProfits = ref(null);
const running = ref(false);
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

const addNew = () => {
  portfolio.value.push({
    coin: "",
    allocation: 0
  })
}

const parseDate = (str) => {
  var y = str.substr(0, 4),
    m = str.substr(4, 2) - 1,
    d = str.substr(6, 2);
  var D = new Date(y, m, d);
  return D.getFullYear() == y && D.getMonth() == m && D.getDate() == d
    ? D
    : "invalid date";
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

const run = () => {
  running.value = true;
  portfolioProfits.value = null;
  if (amount.value < 1) {
    err.value = "DCA amount must be greater than $1";
    running.value = false;
    return;
  }
  if (interval.value != "weekly" && interval.value != "monthly") {
    err.value = "DCA interval must be weekly or monthly";
    running.value = false;
    return;
  }
  if (end.value < start.value) {
    err.value = "Start date can't be sooner than end date";
    running.value = false;
    return;
  }
  err.value = "";
  if (totalPercent.value != 100)
    err.value = "Portfolio allocation must be 100%";

  var coinStr = encodeURIComponent(JSON.stringify(portfolio.value));
  var startDate = new Date(start.value)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var endDate = new Date(end.value)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var url = `dca?allocation=${coinStr}&amnt=${amount.value}&interval=${interval.value}&start=${startDate}&end=${endDate}`;
  http.get(url).then((res) => {
    portfolioProfits.value = res.data.snapshots.map((x) => ({
      ...x,
      value: x.value.toFixed(2),
      profit: x.profit.toFixed(2),
    }));
    running.value = false;
    coinProfits.value = res.data.coins.sort((a, b) => b.profit - a.profit);

    chartData.value = {
      labels: portfolioProfits.value.map((x) =>
        new Date(x.date).toLocaleDateString("en-GB")
      ),
      datasets: [
        {
          data: portfolioProfits.value.map((x) => x.value),
          borderColor: "rgba(0,0,0,0.3)",
          borderWidth: 2,
          backgroundColor: portfolioProfits.value.map((x) =>
            getColor(x.value, x.spent)
          ),
        },
      ],
    };
  });
};
</script>
