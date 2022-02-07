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
          <n-select
            v-model:value="interval"
            :options="Helpers.dcaOptions"
            class="w-1/2"
          />
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
        class="p-6 font-bold flex-col w-full items-center justify-center hidden sm:flex"
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
          <n-select
            v-model:value="interval"
            :options="Helpers.dcaOptions"
            class="w-1/5"
          />
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
            class="flex border border-solid border-slate-300 items-center bg-white w-96"
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
      v-if="result || running"
    >
      <Result :result="result" :running="running" />
    </div>
  </div>
</template>

<script setup>
import { inject, ref, computed } from "vue";
import Helpers from "../helpers";
import Result from "../components/Result.vue";

import {
  NInput,
  NInputNumber,
  NSelect,
  NDatePicker,
  NButton,
  NTable,
  NInputGroup,
} from "naive-ui";

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

const coinOpts = computed(() =>
  props.coins
    .filter((x) => !search || x.toLowerCase().includes(search.value.toLowerCase()))
    .sort((a, b) => a.localeCompare(b))
    .filter(Helpers.onlyUnique)
    .map((x) => ({
      label: Helpers.toCaps(x.replace(/-/g, " ")),
      value: x,
    }))
);

const totalPercent = computed(() =>
  portfolio.value.reduce((a, b) => {
    return a + b["allocation"];
  }, 0)
);

const allocationColor = computed(() =>
  totalPercent.value <= 100 ? "#31a843" : "#c93a5c"
);

const addNew = () =>
  portfolio.value.push({
    coin: "",
    allocation: 0,
  });

const parseDate = (str) => {
  var y = str.substr(0, 4),
    m = str.substr(4, 2) - 1,
    d = str.substr(6, 2);
  var D = new Date(y, m, d);
  return D.getFullYear() == y && D.getMonth() == m && D.getDate() == d
    ? D
    : "invalid date";
};

const running = ref(false)
const result = ref({})

const run = () => {
  running.value = true;

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
    result.value = res.data;
    running.value = false;
  });
};
</script>
