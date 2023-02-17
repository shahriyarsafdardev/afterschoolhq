import React from 'react';
import { FormControlLabel, Checkbox, Grid } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import useStyles from './styles';

function CheckBox({
  option, click, checked, formik,
}) {
  const classes = useStyles();
  const handleCheck = ({ target: { checked } }) => {
    click(checked, option);
  };

  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid} />
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <div className={classes.checkboxWrapper}>
          <FormControlLabel
            control={(
              <Checkbox
                checked={checked}
                classes={{
                  checked: classes.checked,
                  root: classes.root,
                }}
                checkedIcon={(
                  <CheckIcon style={{
                    height: '18px',
                    width: '18px',
                  }}
                  />
                )}
                IconStyle={{ fill: 'black' }}
              />
            )}
            label={option.value}
            classes={{
              root: classes.formControlLabelRoot,
            }}
            onChange={handleCheck}
          />
        </div>
      </Grid>
    </Grid>
  );
}

export default CheckBox;
