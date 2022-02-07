<template>
  <div>
    <div
      class="w-full rounded bg-gray-200 flex justify-center flex-col items-center"
    >
      <p class="p-6 text-lg">
        This strategy backtests buying a set amount of a coin if it enters the selected top coins and then holding to the end date.
      </p>
      <div class="h-px w-10/12 bg-gray-800" />
      <p class="text-red-700 font-bold mt-6" v-if="err">{{ err }}</p>
      <div class="flex flex-col sm:hidden p-6 w-full text-left justify-center">
        <div class="w-full flex items-center justify-center">
          <p class="w-1/2">Purchase Amount</p>
          <n-input-number
            v-model:value="amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center justify-center">
          <p class="w-1/2">Top # of coins</p>
          <n-input-number
            v-model:value="topNum"
            placeholder="top # of coins"
            :min="1"
            :max="500"
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
          <p class="w-1/5">Purchase Amount</p>
          <p class="w-1/5">Top # of coins</p>
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
          <n-input-number
            v-model:value="topNum"
            placeholder="Top # of coins"
            :min="1"
            :max="500"
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
        <n-button type="info" class="bg-blue-600 ml-2 mt-4 w-48" @click="run">
          Run
        </n-button>
      </div>
    </div>

    <div class="w-full rounded bg-gray-200 mt-6" v-if="result || running">
      <Result :result="result" :running="running"/>
    </div>
  </div>
</template>

<script setup>
import { inject, ref } from "vue";
const http = inject("$http");
import { Chart, registerables } from "chart.js";
import { LineChart } from "vue-chart-3";
import {
  NInput,
  NInputNumber,
  NSelect,
  NDatePicker,
  NButton,
  NInputGroup
} from "naive-ui";
import Result from "../components/Result.vue"
Chart.register(...registerables);

const err = ref(null)
const dateDisabled = (ts) => ts < new Date("2013-01-01");
const amount = ref(100)
const topNum = ref(100)
const start = ref(Date.now() - 86400000 * 7);
const end = ref(Date.now());

const running = ref(false)
const result = ref({})

const run = ()=> {
  err.value = ""
  if(topNum.value > 500) {
    err.value = "Only the top 500 coins are known about, please select a number between 0-500"
     running.value = false;
    return;
  }
  if (amount.value < 1) {
    err.value = "DCA amount must be greater than $1";
    running.value = false;
    return;
  }
  if (end.value < start.value) {
    err.value = "Start date can't be sooner than end date";
    running.value = false;
    return;
  }

  err.value = "";
  var startDate = new Date(start.value)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var endDate = new Date(end.value)
    .toISOString()
    .slice(0, 10)
    .replace(/-/g, "");
  var url = `onetimetop?amnt=${amount.value}&numCoins=${topNum.value}&start=${startDate}&end=${endDate}`;
  http.get(url).then(res=>{
    running.value = false;
    result.value = res.data
  })
}
</script>
