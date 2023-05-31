import React, { useEffect, useState } from 'react';
import useStyles from './styles';

const Timer = ({ isRunning }) => {
  const classes = useStyles();
  const [second, setSecond] = useState(0);
  const [minute, setMinute] = useState(0);
  useEffect(() => {
    if (isRunning) {
      setTimeout(() => {
        if (second === 59) {
          setSecond(0);
          setMinute(prevState => prevState + 1);
        } else {
          setSecond(second + 1);
        }
      }, 1000);
    }
  }, [second, isRunning, minute]);

  const getTimerValue = () => {
    if (second < 10) {
      return `${minute}:0${second}`;
    } else {
      return `${minute}:${second}`;
    }
  };
  return <div className={classes.wrapper}>{getTimerValue()}</div>;
};

export default Timer;
