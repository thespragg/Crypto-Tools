<template>
  <div>
    <n-spin :size="100" class="p-12" v-if="running">
      <template #description> Running Backtest </template>
    </n-spin>
    <div
      v-if="portfolioProfits"
      class="flex justify-center flex-col items-center"
    >
      <p>Months profitable: {{ profitablePercent }}%</p>
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
          <tr v-for="month in portfolioProfits" :key="month.date">
            <td>{{ new Date(month.date).toLocaleDateString("en-GB") }}</td>
            <td>
              {{ month.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") }}
            </td>
            <td>
              {{ month.spent.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") }}
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
</template>

<script setup>
import { ref, computed } from "vue";
import { Chart, registerables } from "chart.js";
import { LineChart } from "vue-chart-3";
import { NSpin, NTable } from "naive-ui";
import Helpers from "../helpers";
Chart.register(...registerables);

const props = defineProps({
  result: {
    type: Object,
    required: false,
    default: () => {},
  },
  running: {
    type: Boolean,
    required: true,
    default: () => false,
  },
});

const portfolioProfits = computed(() => {
  if (!props.result || !props.result.snapshots) return null;
  return props.result.snapshots.map((x) => ({
    ...x,
    value: x.value.toFixed(2),
    profit: x.profit.toFixed(2),
  }));
});
const chartData = computed(()=>{
    return {
      labels: portfolioProfits.value.map((x) =>
        new Date(x.date).toLocaleDateString("en-GB")
      ),
      datasets: [
        {
          data: portfolioProfits.value.map((x) => x.value),
          borderColor: "rgba(0,0,0,0.3)",
          borderWidth: 2,
          backgroundColor: portfolioProfits.value.map((x) =>
            Helpers.getColor(x.value, x.spent)
          ),
        },
      ],
    }
})
const coinProfits = computed(() => {
  if (!props.result || !props.result.coins) return {};
  return props.result.coins.sort((a, b) => b.profit - a.profit);
});
const profitablePercent = computed(() => {
  if (!props.result || !props.result.snapshots) return 0;
  var profitable = props.result.snapshots.filter((x) => x.profit > 0).length;
  return ((profitable / props.result.snapshots.length) * 100).toFixed(2);
});

if (!props.running) {
  //   chartData.value = 
}
</script>
