<template>
  <div class="flex justify-center flex-col p-6">
    <div
      class="w-full rounded bg-gray-200 flex justify-center flex-col items-center"
    >
      <p class="p-6 text-lg">
        This strategy simulates what dollar cost averaging into the top coins by
        market cap over time would be.
      </p>
      <div class="h-px w-10/12 bg-gray-800" />
      <p class="text-red-700 font-bold mt-6" v-if="err">{{ err }}</p>
      <div class="flex flex-col sm:hidden p-6 w-full text-left">
        <div class="w-full flex items-center">
          <p class="w-1/2">DCA Amount</p>
          <n-input-number
            v-model:value="etf.amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">Top # of coins</p>
          <n-input-number
            v-model:value="etf.coins"
            placeholder="# of coins"
            :min="1"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">DCA Interval</p>
          <n-select
            v-model:value="etf.interval"
            :options="options"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">Start Date</p>
          <n-date-picker
            v-model:value="etf.start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">End Date</p>
          <n-date-picker
            v-model:value="etf.end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
      </div>
      <div class="p-6 font-bold flex-col w-full items-center hidden sm:flex">
        <div class="flex w-full mb-2 items-center">
          <p class="w-1/5">DCA Amount</p>
          <p class="w-1/5">Top # of coins</p>
          <p class="w-1/5">DCA Interval</p>
          <p class="w-1/5">Start Date</p>
          <p class="w-1/5">End Date</p>
        </div>
        <n-input-group>
          <n-input-number
            v-model:value="etf.amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/5"
          />
          <n-input-number
            v-model:value="etf.coins"
            placeholder="# of coins"
            :min="1"
            class="w-1/5"
          />
          <n-select
            v-model:value="etf.interval"
            :options="options"
            class="w-1/5"
          />
          <n-date-picker
            v-model:value="etf.start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
          <n-date-picker
            v-model:value="etf.end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
        </n-input-group>
        <div class="flex w-full mt-2 items-center">
          <p class="mr-2">Ignored coins:</p>
          <div
            v-for="coin in ignoredCoins"
            :key="coin"
            class="mr-4 font-normal border border-solid border-slate-500 p-2 flex items-center"
          >
            <p class="mr-2">{{ coin }}</p>
            <span class="w-px h-full mr-2">|</span>
            <p class="text-red-600 cursor-pointer" @click="deleteCoin(coin)">x</p>
          </div>
          <n-input-group class="w-96">
            <n-select
              v-model:value="newIngoreCoin"
              :options="coinOpts"
              placeholder="Coin"
              class="w-3/5"
            >
              <template #action>
                <n-input v-model:value="search" placeholder="search" />
              </template>
            </n-select>
            <n-button class="bg-white" @click="addIgnore">Add</n-button>
          </n-input-group>
        </div>
        <n-button type="info" class="bg-blue-600 ml-2 mt-4 w-48" @click="run">
          Run
        </n-button>
      </div>
    </div>

    <div class="w-full rounded bg-gray-200 mt-6" v-if="portfolio || running">
      <n-spin :size="100" class="p-12" v-if="running">
        <template #description> Running Backtest </template>
      </n-spin>
      <div v-if="portfolio" class="flex justify-center flex-col items-center">
        <LineChart
          :chartData="chartData"
          class="w-11/12 h-96"
          :options="Helpers.chartOpts"
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
            <tr v-for="month in portfolio" :key="month.date">
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
                :style="{ color: Helpers.getProfitColor(month.profit) }"
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
                :style="{ color: Helpers.getProfitColor(coin.profit) }"
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
import Helpers from "../helpers"

const http = inject("$http");

const props = defineProps({
  coins: {
    required: true,
    type: Array,
  },
});
const search = ref("");
const newIngoreCoin = ref("");
const ignoredCoins = ref([]);
const addIgnore = () => {
  ignoredCoins.value.push(newIngoreCoin.value);
  newIngoreCoin.value = "";
};

const coinOpts = computed(() =>
  props.coins
    .filter(
      (x) => !search || x.toLowerCase().includes(search.value.toLowerCase())
    )
    .sort((a, b) => a.localeCompare(b))
    .filter(Helpers.onlyUnique)
    .map((x) => ({
      label: Helpers.toCaps(x.replace(/-/g, " ")),
      value: x,
    }))
);

const deleteCoin = (coin) => {
  ignoredCoins.value.splice(ignoredCoins.value.indexOf(coin), 1)
}
const etf = ref({
  amount: 100,
  coins: 100,
  interval: "monthly",
  start: Date.now() - 86400000 * 7,
  end: Date.now(),
});

const running = ref(false);
const err = ref(null);
const dateDisabled = (ts) => ts < new Date("2013-01-01");

const parseDate = (str) => {
  var y = str.substr(0, 4),
    m = str.substr(4, 2) - 1,
    d = str.substr(6, 2);
  var D = new Date(y, m, d);
  return D.getFullYear() == y && D.getMonth() == m && D.getDate() == d
    ? D
    : "invalid date";
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

const portfolio = ref(null);
const chartData = ref(null);
const coinProfits = ref(null);

const run = () => {
  running.value = true;
  portfolio.value = null;
  if (etf.value.amount < 1) {
    err.value = "DCA amount must be greater than $1";
    running.value = false;
    return;
  }
  if (etf.value.coins > 500) {
    err.value = "# of coins must be less than 500";
    running.value = false;
    return;
  }
  if (etf.value.interval != "weekly" && etf.value.interval != "monthly") {
    err.value = "DCA interval must be weekly or monthly";
    running.value = false;
    return;
  }
  if (etf.value.end < etf.value.start) {
    err.value = "Start date can't be sooner than end date";
    running.value = false;
    return;
  }
  err.value = "";
  var start = new Date(etf.value.start)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var end = new Date(etf.value.end)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var url = `etf?amnt=${etf.value.amount}&coins=${etf.value.coins}&interval=${etf.value.interval}&start=${start}&end=${end}`;
  if(ignoredCoins.value != null && ignoredCoins.value.length > 0) url += `&ignored=${ignoredCoins.value.join(',')}`
  http.get(url).then((res) => {
    portfolio.value = res.data.snapshots.map((x) => ({
      ...x,
      value: x.value.toFixed(2),
      profit: x.profit.toFixed(2),
    }));
    running.value = false;
    coinProfits.value = res.data.coins.sort((a, b) => b.profit - a.profit);

    chartData.value = {
      labels: portfolio.value.map((x) =>
        new Date(x.date).toLocaleDateString("en-GB")
      ),
      datasets: [
        {
          data: portfolio.value.map((x) => x.value),
          borderColor: "rgba(0,0,0,0.3)",
          borderWidth: 2,
          backgroundColor: portfolio.value.map((x) =>
            Helpers.getColor(x.value, x.spent)
          ),
        },
      ],
    };
  });
};
</script>
