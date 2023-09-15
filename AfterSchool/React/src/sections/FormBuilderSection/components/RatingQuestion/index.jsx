import React, { useEffect, useState } from 'react';
import { TextField, Grid } from '@material-ui/core';

import useStyles from '../../styles';

const RatingQuestion = props => {
  const classes = useStyles();
  const { options, setOptions } = props;
  const { 0: min, length, [length - 1]: max } = options;

  const [values, setValues] = useState({
    minLabel: min?.value || '',
    minValue: Number(min?.key) || 0,
    maxLabel: max?.value || '',
    maxValue: Number(max?.key) || 5
  });

  useEffect(() => {
    const option = [];
    const { minLabel, minValue, maxLabel, maxValue } = values;
    option.push({ key: String(minValue), value: minLabel });
    for (let index = Number(minValue) + 1; index < maxValue; index++) {
      option.push({ key: String(index), value: '' });
    }
    option.push({ key: String(maxValue), value: maxLabel });
    setOptions(option);
  }, [values]);

  const changeHandler = (name, value) => {
    const isNumber = typeof value === 'number';
    if (isNumber) {
      const isMaxValue = name === 'maxValue';

      if (
        isMaxValue ? value <= values.minValue || value > 10 : value >= values.maxValue || value < 0
      ) {
        return;
      }
    }
    setValues({ ...values, [name]: value });
  };

  return (
    <Grid container>
      <Grid item container lg={10} xs={10} sm={10} className={classes.marginAuto}>
        <Grid item lg={6} xs={12} sm={12} className={classes.displayFlex}>
          <TextField
            required
            fullWidth
            size="small"
            type="number"
            name="minValue"
            id="standard-basic"
            value={values.minValue}
            data-testid="min-value"
            placeholder="Minimum Value"
            className={classes.formControlClass}
            InputProps={{ inputProps: { min: 0, max: 9 } }}
            onChange={e => {
              changeHandler(e.target.name, Number(e.target.value));
            }}
          />
        </Grid>
        <Grid item lg={6} xs={12} sm={12}>
          <TextField
            required
            fullWidth
            size="small"
            name="minLabel"
            id="standard-basic"
            value={values.minLabel}
            data-testid="min-value-label"
            className={classes.formControlClass}
            placeholder="Label for Min Value (Optional)"
            onChange={e => {
              changeHandler(e.target.name, e.target.value);
            }}
          />
        </Grid>
      </Grid>
      <Grid item container lg={10} xs={10} sm={10} className={classes.marginAuto}>
        <Grid item lg={6} xs={12} sm={12} className={classes.displayFlex}>
          <TextField
            required
            fullWidth
            size="small"
            type="number"
            name="maxValue"
            id="standard-basic"
            data-testid="max-value"
            value={values.maxValue}
            placeholder="Maximum Value"
            className={classes.formControlClass}
            InputProps={{ inputProps: { min: 1, max: 10 } }}
            onChange={e => {
              changeHandler(e.target.name, Number(e.target.value));
            }}
          />
        </Grid>
        <Grid item lg={6} xs={12} sm={12}>
          <TextField
            required
            fullWidth
            size="small"
            name="maxLabel"
            id="standard-basic"
            value={values.maxLabel}
            data-testid="max-value-label"
            className={classes.formControlClass}
            placeholder="Label for Max Value (Optional)"
            onChange={e => {
              changeHandler(e.target.name, e.target.value);
            }}
          />
        </Grid>
      </Grid>
    </Grid>
  );
};

export default RatingQuestion;
